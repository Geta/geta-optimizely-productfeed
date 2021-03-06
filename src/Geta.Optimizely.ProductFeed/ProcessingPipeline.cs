// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Repositories;

namespace Geta.Optimizely.ProductFeed
{
    public class ProcessingPipeline<TEntity>
    {
        private readonly ISiteBuilder _siteBuilder;
        private readonly IProductFeedContentLoader _feedContentLoader;
        private readonly IEntityMapper<TEntity> _entityMapper;
        private readonly IEnumerable<IProductFeedContentEnricher<TEntity>> _enrichers;
        private readonly IEnumerable<FeedDescriptor> _feedDescriptors;
        private readonly Func<FeedDescriptor, AbstractFeedContentExporter<TEntity>> _converterFactory;
        private readonly IFeedRepository _feedRepository;
        private readonly IProductFeedFilter<TEntity> _filter;

        public ProcessingPipeline(
            ISiteBuilder siteBuilder,
            IProductFeedContentLoader feedContentLoader,
            IEntityMapper<TEntity> entityMapper,
            IEnumerable<IProductFeedContentEnricher<TEntity>> enrichers,
            IEnumerable<FeedDescriptor> feedDescriptors,
            Func<FeedDescriptor, AbstractFeedContentExporter<TEntity>> converterFactory,
            IFeedRepository feedRepository,
            IProductFeedFilter<TEntity> filter = null)
        {
            _siteBuilder = siteBuilder;
            _feedContentLoader = feedContentLoader;
            _entityMapper = entityMapper;
            _enrichers = enrichers;
            _feedDescriptors = feedDescriptors;
            _converterFactory = converterFactory;
            _feedRepository = feedRepository;
            _filter = filter;
        }

        public void Process(JobStatusLogger logger, CancellationToken cancellationToken)
        {
            logger.LogWithStatus(
                $"Found {_feedDescriptors.Count()} ({string.Join(", ", _feedDescriptors.Select(f => f.Name))}) feeds. Build process starting...");

            var exporters = _feedDescriptors
                .Select(d => _converterFactory(d))
                .ToList();

            var sourceData = _feedContentLoader
                .LoadSourceData(cancellationToken)
                .Select(d => _entityMapper.Map(d))
                .Where(d => _filter?.ShouldInclude(d) ?? true)
                .Where(d => d != null)
                .Select(d =>
                {
                    foreach (var enricher in _enrichers)
                    {
                        enricher.Enrich(d, cancellationToken);
                    }

                    return d;
                })
                .ToList();

            foreach (var host in _siteBuilder.GetHosts())
            {
                // begin exporting pipeline - this is good moment for some of the exporters to prepare file headers
                // render document start tag or do some other magic
                foreach (var exporter in exporters)
                {
                    exporter.BeginExport(host, cancellationToken);
                }

                foreach (var d in sourceData)
                {
                    foreach (var exporter in exporters)
                    {
                        exporter.BuildEntry(d, host, cancellationToken);
                    }
                }

                // dispose exporters - so we let them flush and wrap-up
                foreach (var exporter in exporters)
                {
                    _feedRepository.Save(exporter.FinishExport(host, cancellationToken));
                }

                logger.LogWithStatus($"> Generated feeds for {host.Url} host.");
            }

            // let it go! let it go!
            foreach (var exporter in exporters)
            {
                if (exporter is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            logger.LogWithStatus("Product feed generation completed.");
        }
    }
}
