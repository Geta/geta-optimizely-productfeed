// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using Geta.Optimizely.GoogleProductFeed.Repositories;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed.Repositories
{
    public class FeedRepository : IFeedRepository
    {
        private readonly FeedApplicationDbContext _applicationDbContext;
        private readonly IEnumerable<FeedDescriptor> _descriptors;

        public FeedRepository(FeedApplicationDbContext applicationDbContext, IEnumerable<FeedDescriptor> descriptors)
        {
            _applicationDbContext = applicationDbContext;
            _descriptors = descriptors;
        }

        public FeedMedia GetLatestFeed(Uri siteHost)
        {
            if (siteHost == null)
            {
                throw new ArgumentNullException(nameof(siteHost));
            }

            var feedContent = _applicationDbContext
                .FeedData
                .Where(f => f.Link.Contains(siteHost.AbsolutePath))
                .OrderByDescending(f => f.CreatedUtc)
                .FirstOrDefault();

            return new FeedMedia(feedContent?.FeedBytes, FindDescriptorByUrl(siteHost.PathAndQuery.TrimStart('/')));
        }

        public void Save(ICollection<FeedEntity> feedData)
        {
            if (feedData == null)
            {
                return;
            }

            foreach (var data in feedData)
            {
                var found = _applicationDbContext.FeedData.FirstOrDefault(f => f.Link.Contains(data.Link));

                if (found != null)
                {
                    found.CreatedUtc = DateTime.UtcNow;
                    found.FeedBytes = data.FeedBytes;
                }
                else
                {
                    data.CreatedUtc = DateTime.UtcNow;
                    _applicationDbContext.FeedData.Add(data);
                }

                _applicationDbContext.SaveChanges();
            }
        }

        private FeedDescriptor FindDescriptorByUrl(string pathAndQuery)
        {
            return _descriptors.FirstOrDefault(d => d.FileName.TrimStart('/')
                                                   .Equals(pathAndQuery, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
