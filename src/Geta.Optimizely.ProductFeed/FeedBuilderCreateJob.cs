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
using Microsoft.Extensions.Logging;

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
        private readonly Func<Type, IProductFeedContentConverter> _converterFactory;
        private readonly IFeedRepository _feedRepository;
        private readonly ILogger<FeedBuilderCreateJob> _logger;
        private readonly JobStatusLogger _jobStatusLogger;
        private bool _stopped;

        public FeedBuilderCreateJob(IProductFeedContentLoader feedContentLoader,
            IEnumerable<IProductFeedContentEnricher> enrichers,
            IEnumerable<FeedDescriptor> feedDescriptors,
            Func<Type, IProductFeedContentConverter> converterFactory,
            IFeedRepository feedRepository,
            ILogger<FeedBuilderCreateJob> logger)
        {
            _feedContentLoader = feedContentLoader;
            _enrichers = enrichers;
            _feedDescriptors = feedDescriptors;
            _converterFactory = converterFactory;
            _feedRepository = feedRepository;
            _logger = logger;
            _jobStatusLogger = new JobStatusLogger(OnStatusChanged);

            IsStoppable = true;
        }

        public override string Execute()
        {
            var ct = CancellationToken.None;
            var feedSourceData = _feedContentLoader.LoadSourceData(ct);
            var successCount = 0;

            foreach (var enricher in _enrichers)
            {
                enricher.Enrich(feedSourceData, ct);
            }

            _jobStatusLogger.LogWithStatus(
                $"Found {_feedDescriptors.Count()} ({string.Join(", ", _feedDescriptors.Select(f => f.Name))}) feeds. Build process starting...");

            foreach (var feedDescriptor in _feedDescriptors)
            {
                if (_stopped)
                {
                    _jobStatusLogger.Log("Job was stopped");
                    return _jobStatusLogger.ToString();
                }

                _jobStatusLogger.LogWithStatus($"Building {feedDescriptor.Name} feed...");

                try
                {
                    var converter = _converterFactory(feedDescriptor.Converter);
                    if (converter == null)
                    {
                        throw new InvalidOperationException($"Field converter for `{feedDescriptor.Name}` feed is not configured");
                    }

                    var result = converter.Convert(feedSourceData, feedDescriptor, ct);

                    _feedRepository.Save(result);

                    successCount++;
                }
                catch (Exception ex)
                {
                    _jobStatusLogger.LogWithStatus($"Failed to build `{feedDescriptor.Name}` product feed. Exception: {ex}");

                    // save to technical log file as well
                    _logger.LogError(ex, $"Failed to build `{feedDescriptor.Name}` product feed");
                }
            }

            _jobStatusLogger.LogWithStatus($"Job executed. Feeds {successCount} out of {_feedDescriptors.Count()} created and saved to the storage.");

            return _jobStatusLogger.ToString();
        }

        public override void Stop()
        {
            _stopped = true;
        }
    }
}
