// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed.Repositories
{
    public interface IFeedRepository
    {
        void RemoveOldVersions(int numberOfGeneratedFeeds);

        FeedData GetLatestFeedData(string siteHost);

        void Save(ICollection<FeedData> feedData);
    }
}
