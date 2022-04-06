// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed
{
    public interface IProductFeedConverter
    {
        IFeed CreateFeed(FeedDescriptor feedDescriptor);

        IFeedEntry Convert(CatalogContentBase catalogContent);
    }
}
