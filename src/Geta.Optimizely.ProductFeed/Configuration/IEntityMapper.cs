// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using EPiServer.Commerce.Catalog.ContentTypes;

namespace Geta.Optimizely.ProductFeed.Configuration;

public interface IEntityMapper<out TEntity>
{
    public TEntity Map(CatalogContentBase catalogContent);
}
