// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Threading;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.PlugIn;
using EPiServer.Scheduler;

namespace Geta.Optimizely.ProductFeed
{
    [ScheduledPlugIn(
        GUID = "F270B8BF-7B25-4254-B4D3-4FE843B2D559",
        DisplayName = "ProductFeed - Create feeds",
        Description = "Creates and stores product feeds")]
    public class FeedBuilderCreateJob : ScheduledJobBase
    {
        private readonly ServiceFactory _serviceFactory;
        private readonly JobStatusLogger _jobStatusLogger;
        private readonly CancellationTokenSource _cts = new ();

        public FeedBuilderCreateJob(ServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            _jobStatusLogger = new JobStatusLogger(OnStatusChanged);

            IsStoppable = true;
        }

        public override string Execute()
        {
            try
            {
                var mappedType = typeof(CatalogContentBase);
                var genericPipelineType = typeof(ProcessingPipeline<>).MakeGenericType(mappedType);
                var genericPipeline = _serviceFactory(genericPipelineType);
                var mi = genericPipelineType.GetMethod("Process");

                if (mi != null)
                {
                    mi.Invoke(genericPipeline, new object[] { _jobStatusLogger, _cts.Token });
                }
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
