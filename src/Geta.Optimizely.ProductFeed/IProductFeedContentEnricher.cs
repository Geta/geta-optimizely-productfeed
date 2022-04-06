// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Threading;
using EPiServer.Commerce.Catalog.ContentTypes;

namespace Geta.Optimizely.ProductFeed
{
    public interface IProductFeedContentEnricher
    {
        CatalogContentBase Enrich(CatalogContentBase sourceData, CancellationToken cancellationToken);
    }

    internal class DefaultIProductFeedContentEnricher : IProductFeedContentEnricher
    {
        public CatalogContentBase Enrich(CatalogContentBase sourceData, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return sourceData;
        }
    }
}
