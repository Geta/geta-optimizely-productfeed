// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geta.Optimizely.GoogleProductFeed;
using Geta.Optimizely.ProductFeed.Infrastructure;
using Geta.Optimizely.ProductFeed.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Geta.Optimizely.ProductFeed.Google
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
        public async Task<IActionResult> Get()
        {
            var siteHost = HttpContext.Request.Host.ToString();
            var feed = _feedHelper.GetLatestFeed(siteHost);

            if (feed == null)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Content = ObjectXmlSerializer.Serialize("No feed generated", typeof(string)),
                    ContentType = "application/xml"
                };
            }

            feed.Entries = feed.Entries
                .Where(e => e.Link.Contains(siteHost))
                .ToList();

            return Content(ObjectXmlSerializer.Serialize(feed, typeof(Feed)), "application/xml", Encoding.UTF8);
        }
    }
}
