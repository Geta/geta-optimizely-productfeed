// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Geta.Optimizely.ProductFeed.Configuration
{
    public static class IEndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapProductFeeds(this IEndpointRouteBuilder endpoints)
        {
            var descriptors = endpoints.ServiceProvider.GetServices<FeedDescriptor>();

            foreach (var descriptor in descriptors)
            {
                endpoints.MapControllerRoute(
                    descriptor.Name + "-endpoint",
                    descriptor.FileName.TrimStart('/'),
                    new
                    {
                        controller = nameof(ProductFeedController).Replace(nameof(Controller), string.Empty),
                        action = nameof(ProductFeedController.Get)
                    });
            }

            return endpoints;
        }
    }
}
