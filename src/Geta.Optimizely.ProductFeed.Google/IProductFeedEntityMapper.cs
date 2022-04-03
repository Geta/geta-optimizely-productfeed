// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Google.Models;

namespace Geta.Optimizely.ProductFeed.Google
{
    public interface IProductFeedEntityMapper
    {
        Feed GenerateFeedEntity(FeedDescriptor feedDescriptor);

        Entry GenerateEntry(CatalogContentBase catalogContent);
    }
}
