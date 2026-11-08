// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using Geta.Optimizely.ProductFeed.Repositories;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Geta.Optimizely.ProductFeed;

public class ProductFeedController(IFeedRepository feedRepository) : ControllerBase
{
    public IActionResult Get()
    {
        var host = HttpContext.Request.GetEncodedUrl();
        var siteHost = new Uri(host);
        var descriptor = feedRepository.FindDescriptorByUri(siteHost);

        if (descriptor == null)
        {
            return NotFound("Feed descriptor not found");
        }

        var feedInfo = feedRepository.GetLatestFeed(siteHost);

        if (feedInfo == null)
        {
            return NotFound("Feed not found");
        }

        return File(feedInfo.FeedBytes, descriptor.MimeType);
    }
}
