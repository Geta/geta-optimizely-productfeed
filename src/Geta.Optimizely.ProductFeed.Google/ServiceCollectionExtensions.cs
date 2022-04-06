// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using Geta.Optimizely.ProductFeed.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Geta.Optimizely.ProductFeed.Google
{
    public static class ProductFeedOptionsExtensions
    {
        public static ProductFeedOptions AddGoogleExport(
            this ProductFeedOptions options,
            Action<FeedDescriptor> setupAction)
        {
            var descriptor = new GoogleFeedDescriptor();
            setupAction(descriptor);
            options.Add(descriptor);

            return options;
        }
    }
}
