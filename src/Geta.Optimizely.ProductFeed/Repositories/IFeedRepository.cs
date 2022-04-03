// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed.Repositories
{
    public interface IFeedRepository
    {
        FeedMedia GetLatestFeed(Uri siteHost);

        void Save(ICollection<FeedEntity> feedData);
    }
}
