// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Threading;
using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Google.Models;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed.Google
{
    public class GoogleProductFeedExporter : AbstractFeedContentExporter
    {
        private readonly List<Entry> _entries = new();

        public override ICollection<FeedEntity> Export(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var f = (Feed)Converter.CreateFeed(Descriptor);
            f.Entries = _entries;

            return new[]
            {
                new FeedEntity
                {
                    CreatedUtc = f.Updated, Link = f.Link, FeedBytes = ObjectXmlSerializer.Serialize(f, typeof(Feed))
                }
            };
        }

        public override void ConvertEntry(CatalogContentBase catalogContentBase, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _entries.Add((Entry)Converter.Convert(catalogContentBase));
        }
    }
}
