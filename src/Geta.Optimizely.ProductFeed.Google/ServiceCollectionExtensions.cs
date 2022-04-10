// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using Geta.Optimizely.ProductFeed.Configuration;

namespace Geta.Optimizely.ProductFeed.Google
{
    public static class ProductFeedOptionsExtensions
    {
        public static ProductFeedOptions AddGoogleExport(
            this ProductFeedOptions options,
            Action<FeedDescriptor> setupAction)
        {
            var descriptor = new GoogleFeedDescriptor();

            descriptor.SetExporter<GoogleProductFeedExporter>();
            descriptor.SetSiteUrlBuilder<DefaultSiteUrlBuilder>();

            setupAction(descriptor);
            options.Add(descriptor);

            return options;
        }
    }
}
