// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Repositories;

namespace Geta.Optimizely.ProductFeed
{
    [ScheduledPlugIn(
        GUID = "F270B8BF-7B25-4254-B4D3-4FE843B2D559",
        DisplayName = "ProductFeed - Create feeds",
        Description = "Creates and stores product feeds")]
    public class FeedBuilderCreateJob : ScheduledJobBase
    {
        private readonly IProductFeedContentLoader _feedContentLoader;
        private readonly IEnumerable<IProductFeedContentEnricher> _enrichers;
        private readonly IEnumerable<FeedDescriptor> _feedDescriptors;
        private readonly Func<FeedDescriptor, IProductFeedContentExporter> _converterFactory;
        private readonly IFeedRepository _feedRepository;
        private readonly JobStatusLogger _jobStatusLogger;
        private readonly CancellationTokenSource _cts = new ();

        public FeedBuilderCreateJob(IProductFeedContentLoader feedContentLoader,
            IEnumerable<IProductFeedContentEnricher> enrichers,
            IEnumerable<FeedDescriptor> feedDescriptors,
            Func<FeedDescriptor, IProductFeedContentExporter> converterFactory,
            IFeedRepository feedRepository)
        {
            _feedContentLoader = feedContentLoader;
            _enrichers = enrichers;
            _feedDescriptors = feedDescriptors;
            _converterFactory = converterFactory;
            _feedRepository = feedRepository;
            _jobStatusLogger = new JobStatusLogger(OnStatusChanged);

            IsStoppable = true;
        }

        public override string Execute()
        {
            try
            {
                var converters = _feedDescriptors
                    .Select(d => _converterFactory(d))
                    .ToList();

                var sourceData = _feedContentLoader
                    .LoadSourceData(_cts.Token)
                    .Select(d =>
                    {
                        foreach (var enricher in _enrichers)
                        {
                            enricher.Enrich(d, _cts.Token);
                        }

                        return d;
                    });

                foreach (var d in sourceData)
                {
                    foreach (var converter in converters)
                    {
                        converter.ConvertEntry(d, _cts.Token);
                    }
                }

                // dispose exporters - so we let them flush and wrap-up
                foreach (var converter in converters)
                {
                    _feedRepository.Save(converter.Export(_cts.Token));

                    if (converter is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }

                _jobStatusLogger.LogWithStatus(
                    $"Found {_feedDescriptors.Count()} ({string.Join(", ", _feedDescriptors.Select(f => f.Name))}) feeds. Build process starting...");
            }
            catch (Exception ex)
            {
                _jobStatusLogger.Log($"Error occurred. {ex}");
            }

            _jobStatusLogger.LogWithStatus("Job finished.");

            return _jobStatusLogger.ToString();
        }

        public override void Stop()
        {
            _cts.Cancel();
        }
    }
}
