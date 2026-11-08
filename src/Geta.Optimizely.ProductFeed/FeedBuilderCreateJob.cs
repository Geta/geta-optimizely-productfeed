// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Threading;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using Geta.Optimizely.ProductFeed.Configuration;

namespace Geta.Optimizely.ProductFeed;

[ScheduledPlugIn(
    GUID = "F270B8BF-7B25-4254-B4D3-4FE843B2D559",
    DisplayName = "ProductFeed - Create feeds",
    Description = "Creates and stores product feeds")]
public class FeedBuilderCreateJob : ScheduledJobBase
{
    private readonly ServiceFactory _serviceFactory;
    private readonly ProductFeedOptions _options;
    private readonly JobStatusLogger _jobStatusLogger;
    private readonly CancellationTokenSource _cts = new ();

    public FeedBuilderCreateJob(ServiceFactory serviceFactory, ProductFeedOptions options)
    {
        _serviceFactory = serviceFactory;
        _options = options;
        _jobStatusLogger = new JobStatusLogger(OnStatusChanged);

        IsStoppable = true;
    }

    public override string Execute()
    {
        try
        {
            // glory of the generics
            // this is required as we have no idea until the very last minute (runtime) what entity type consuming project will choose
            // so to make it easier for the ProductFeed library - whole processing pipeline is generalized with type parameter
            // but from the point of view of the scheduled job - we have no info about generic type we should use here
            // thus - resolve the closed pipeline type at runtime and call it through its non-generic interface
            var mappedType = _options.MappedEntity;
            var genericPipelineType = typeof(ProcessingPipeline<>).MakeGenericType(mappedType);
            var pipeline = (IProcessingPipeline)_serviceFactory(genericPipelineType);

            pipeline.Process(_jobStatusLogger, _cts.Token);
        }
        catch (OperationCanceledException) when (_cts.IsCancellationRequested)
        {
            _jobStatusLogger.LogWithStatus("Job was stopped before feed generation completed.");
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
        // Not disposed: Execute may still read the token, and the scheduler can call Stop more than once.
        _cts.Cancel();
    }
}
