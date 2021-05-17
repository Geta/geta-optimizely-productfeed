// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Linq;
using System.Threading.Tasks;
using Geta.Optimizely.GoogleProductFeed.Models;
using Microsoft.AspNetCore.Mvc;

namespace Geta.Optimizely.GoogleProductFeed.Controllers
{
    [ApiController]
    [Produces("application/xml")]
    public class GoogleProductFeedController : ControllerBase
    {
        private readonly IFeedHelper _feedHelper;

        public GoogleProductFeedController(IFeedHelper feedHelper)
        {
            _feedHelper = feedHelper;
        }

        [Route("googleproductfeed")]
        public async Task<ActionResult<Feed>> Get()
        {
            var siteHost = HttpContext.Request.Host.ToString();
            var feed = _feedHelper.GetLatestFeed(siteHost);

            if (feed == null)
            {
                return NotFound("No feed generated");
                // return Content(HttpStatusCode.NotFound, "No feed generated", new NamespacedXmlMediaTypeFormatter());
            }

            feed.Entries = feed.Entries.Where(e => e.Link.Contains(siteHost)).ToList();

            return feed;
            // return Content(HttpStatusCode.OK, feed, new NamespacedXmlMediaTypeFormatter());
        }
    }
}
