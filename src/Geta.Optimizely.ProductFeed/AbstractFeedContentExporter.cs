// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Threading;
using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed
{
    public abstract class AbstractFeedContentExporter : IProductFeedContentExporter
    {
        public IProductFeedConverter Converter { get; private set; }

        public FeedDescriptor Descriptor { get; private set; }

        public abstract ICollection<FeedEntity> Export(CancellationToken cancellationToken);

        public abstract void ConvertEntry(CatalogContentBase catalogContentBase, CancellationToken cancellationToken);

        public void SetConverter(IProductFeedConverter converter)
        {
            Converter = converter;
        }

        public void SetDescriptor(FeedDescriptor feedDescriptor)
        {
            Descriptor = feedDescriptor;
        }
    }
}
