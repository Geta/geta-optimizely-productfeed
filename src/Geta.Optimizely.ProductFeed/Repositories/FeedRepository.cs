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

        public void RemoveOldVersions(int numberOfGeneratedFeeds)
        {
            var items = _applicationDbContext.FeedData
                                             .Select(x => new
                                             {
                                                 x.Id,
                                                 x.CreatedUtc
                                             }).OrderByDescending(x => x.CreatedUtc).ToList();

            if (items.Count > numberOfGeneratedFeeds)
            {
                for (var i = items.Count - 1; i >= numberOfGeneratedFeeds; i--)
                {
                    var feedData = new FeedEntity { Id = items[i].Id };

                    _applicationDbContext.FeedData.Attach(feedData);
                    _applicationDbContext.FeedData.Remove(feedData);
                }

                _applicationDbContext.SaveChanges();
            }
        }

        public FeedMedia GetLatestFeedData(Uri siteHost)
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

        private FeedDescriptor FindDescriptorByUrl(string pathAndQuery) =>
            _descriptors.FirstOrDefault(d => d.FileName.TrimStart('/').Equals(pathAndQuery, StringComparison.InvariantCultureIgnoreCase));

        public void Save(ICollection<FeedEntity> feedData)
        {
            if (feedData == null)
            {
                return;
            }

            foreach (var data in feedData)
            {
                data.CreatedUtc = DateTime.UtcNow;

                _applicationDbContext.FeedData.Add(data);
                _applicationDbContext.SaveChanges();
            }
        }
    }
}
