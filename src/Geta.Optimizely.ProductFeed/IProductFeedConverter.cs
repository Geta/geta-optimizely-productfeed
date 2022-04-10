// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using EPiServer.Commerce.Catalog.ContentTypes;

namespace Geta.Optimizely.ProductFeed
{
    public interface IProductFeedConverter
    {
        object Convert(CatalogContentBase catalogContent);
    }
}
