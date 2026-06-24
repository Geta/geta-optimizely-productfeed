// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Threading.Tasks;
using Geta.Optimizely.ProductFeed.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
            endpoints.MapGet(descriptor.FileName.TrimStart('/'), WriteFeedAsync);
        }

        return endpoints;
    }

    private static async Task WriteFeedAsync(HttpContext context)
    {
        var feedRepository = context.RequestServices.GetRequiredService<IFeedRepository>();
        var siteUri = new Uri(context.Request.GetEncodedUrl());

        var feed = feedRepository.GetLatestFeed(siteUri);

        if (feed == null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("Feed not found", context.RequestAborted);
            return;
        }

        var descriptor = feedRepository.FindDescriptorByUri(siteUri);

        if (descriptor == null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync("Feed descriptor not found", context.RequestAborted);
            return;
        }

        context.Response.ContentType = descriptor.MimeType;
        await context.Response.Body.WriteAsync(feed.FeedBytes, context.RequestAborted);
    }
}
