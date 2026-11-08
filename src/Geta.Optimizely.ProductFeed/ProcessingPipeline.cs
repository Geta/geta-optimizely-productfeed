// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Repositories;

namespace Geta.Optimizely.ProductFeed;

// Lets FeedBuilderCreateJob run the pipeline for the entity type chosen at startup without reflection.
internal interface IProcessingPipeline
{
    void Process(JobStatusLogger logger, CancellationToken cancellationToken);
}

public class ProcessingPipeline<TEntity>(
    ISiteBuilder siteBuilder,
    IProductFeedContentLoader feedContentLoader,
    IEntityMapper<TEntity> entityMapper,
    IEnumerable<IProductFeedContentEnricher<TEntity>> enrichers,
    IEnumerable<FeedDescriptor> feedDescriptors,
    Func<FeedDescriptor, AbstractFeedContentExporter<TEntity>> converterFactory,
    IFeedRepository feedRepository,
    IProductFeedFilter<TEntity> filter = null) : IProcessingPipeline
{
    public void Process(JobStatusLogger logger, CancellationToken cancellationToken)
    {
        logger.LogWithStatus(
            $"Found {feedDescriptors.Count()} ({string.Join(", ", feedDescriptors.Select(f => f.Name))}) feeds. Build process starting...");

        var exporters = feedDescriptors
            .Select(converterFactory)
            .ToList();

        try
        {
            logger.LogWithStatus("Loading catalog content...");

            var sourceData = feedContentLoader
                .LoadSourceData(cancellationToken)
                .Select(content =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return entityMapper.Map(content);
                })
                .Where(d => d != null)
                .Where(d => filter?.ShouldInclude(d) ?? true)
                .ToList();

            logger.LogWithStatus($"Loaded {sourceData.Count} entities.");

            for (var i = 0; i < sourceData.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var enricher in enrichers)
                {
                    // An enricher may return a new instance (e.g. a record updated with 'with'); null keeps the current one.
                    sourceData[i] = enricher.Enrich(sourceData[i], cancellationToken) ?? sourceData[i];
                }

                logger.Progress("Enriching entities", i + 1, sourceData.Count);
            }

            foreach (var host in siteBuilder.GetHosts())
            {
                var hostUrl = host.Url;
                var buildActivity = $"Building feeds for {hostUrl}";

                // begin exporting pipeline - this is good moment for some of the exporters to prepare file headers
                // render document start tag or do some other magic
                foreach (var exporter in exporters)
                {
                    exporter.BeginExport(host, cancellationToken);
                }

                for (var i = 0; i < sourceData.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (var exporter in exporters)
                    {
                        exporter.BuildEntry(sourceData[i], host, cancellationToken);
                    }

                    logger.Progress(buildActivity, i + 1, sourceData.Count);
                }

                // Don't replace stored feeds with ones a Stop cut short, e.g. when a loader ends early once cancelled
                // and there were no entities to check the token for.
                cancellationToken.ThrowIfCancellationRequested();

                // dispose exporters - so we let them flush and wrap-up
                foreach (var exporter in exporters)
                {
                    feedRepository.Save(exporter.FinishExport(host, cancellationToken));
                }

                logger.LogWithStatus($"> Generated feeds for {hostUrl} host.");
            }

            logger.LogWithStatus("Product feed generation completed.");
        }
        finally
        {
            // let it go! let it go!
            foreach (var exporter in exporters)
            {
                if (exporter is IDisposable disposable)
                {
                    // A failing Dispose must not hide the stop or error that ended the export, or skip the other exporters.
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        logger.Log($"Failed to dispose exporter {exporter.GetType().FullName}. {ex}");
                    }
                }
            }
        }
    }
}
