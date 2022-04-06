// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Threading;
using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed
{
    public interface IProductFeedContentExporter
    {
        void ConvertEntry(CatalogContentBase catalogContentBase, CancellationToken cancellationToken);

        ICollection<FeedEntity> Export(CancellationToken cancellationToken);
    }
}
