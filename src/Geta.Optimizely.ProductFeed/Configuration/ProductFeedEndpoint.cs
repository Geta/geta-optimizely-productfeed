// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Threading.Tasks;
using Geta.Optimizely.ProductFeed.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Geta.Optimizely.ProductFeed.Configuration;

// Request handler for the product feed route endpoints registered by
// IEndpointRouteBuilderExtensions.MapProductFeeds. Kept separate from the
// extension class so the latter stays focused on endpoint registration.
internal static class ProductFeedEndpoint
{
    public static async Task WriteFeedAsync(HttpContext context, IFeedRepository feedRepository)
    {
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
