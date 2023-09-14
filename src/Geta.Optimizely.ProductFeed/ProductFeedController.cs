// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Text;
using Geta.Optimizely.ProductFeed.Repositories;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Geta.Optimizely.ProductFeed
{
    public class ProductFeedController : ControllerBase
    {
        private readonly IFeedRepository _feedRepository;

        public ProductFeedController(IFeedRepository feedRepository)
        {
            _feedRepository = feedRepository;
        }

        public IActionResult Get()
        {
            var host = HttpContext.Request.GetEncodedUrl();
            var siteHost = new Uri(host);
            var feedInfo = _feedRepository.GetLatestFeed(siteHost);

            if (feedInfo == null)
            {
                return NotFound("Feed not found");
            }

            var descriptor = _feedRepository.FindDescriptorByUri(siteHost);

            return Content(Encoding.UTF8.GetString(feedInfo.FeedBytes), descriptor.MimeType);
        }
    }
}
