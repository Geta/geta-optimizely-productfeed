// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using EPiServer.PlugIn;
using EPiServer.Scheduler;
using Geta.Optimizely.GoogleProductFeed;

namespace Geta.Optimizely.ProductFeed
{
    [ScheduledPlugIn(
        GUID = "F270B8BF-7B25-4254-B4D3-4FE843B2D559",
        DisplayName = "ProductFeed - Create feeds",
        Description = "Creates and stores product feeds")]
    public class FeedBuilderCreateJob : ScheduledJobBase
    {
        private readonly IProductFeedBuilder _productFeedBuilder;

        public FeedBuilderCreateJob(IProductFeedBuilder productFeedBuilder)
        {
            _productFeedBuilder = productFeedBuilder;
        }

        public override string Execute()
        {
            var result = _productFeedBuilder.Build();

            return result
                ? "Job successfully executed. Feed created and saved to the database."
                : "Job failed - FeedBuilder.Build() returned null.";
        }
    }
}