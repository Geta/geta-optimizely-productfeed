// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Geta.Optimizely.ProductFeed.Configuration;

public static class IEndpointRouteBuilderExtensions
{
    // Feeds are mapped as lightweight route endpoints (MapGet) rather than controller routes.
    // A controller route (MapControllerRoute) creates/extends a ControllerActionEndpointDataSource;
    // when MapProductFeeds is called outside the same UseEndpoints scope that mapped the controllers
    // (e.g. when a host such as Optimizely Foundation owns its own UseEndpoints), that produces a
    // second controller data source and throws "Duplicate endpoint name". MapGet registers a separate
    // data source containing only the feed endpoints, so it is safe regardless of host integration.
    public static IEndpointRouteBuilder MapProductFeeds(this IEndpointRouteBuilder endpoints)
    {
        var descriptors = endpoints.ServiceProvider.GetServices<FeedDescriptor>();

        foreach (var descriptor in descriptors)
        {
            endpoints.MapGet(descriptor.FileName.TrimStart('/'), ProductFeedEndpoint.WriteFeedAsync);
        }

        return endpoints;
    }
}
