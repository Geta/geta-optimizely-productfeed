// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed
{
    public interface IProductFeedBuilder
    {
        bool Build();

        Feed GetLatestFeed(string siteHost);
    }
}
