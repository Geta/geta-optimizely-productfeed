// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Google.Models;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed.Google
{
    public class GoogleProductFeedExporter<TEntity> : AbstractFeedContentExporter<TEntity>
    {
        private readonly List<Entry> _entries = new();

        public override ICollection<FeedEntity> FinishExport(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var f = new Feed
            {
                Updated = DateTime.UtcNow,
                Title = "Google Product Feed",
                Link = SiteUrlBuilder.BuildUrl().TrimEnd('/') + '/' + Descriptor.FileName.TrimStart('/'),
                Entries = _entries.Where(e => e != null).ToList()
            };

            return new[]
            {
                new FeedEntity
                {
                    CreatedUtc = f.Updated, Link = f.Link, FeedBytes = ObjectXmlSerializer.Serialize(f, typeof(Feed))
                }
            };
        }

        public override object ConvertEntry(TEntity catalogContentBase, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = (Entry)Converter.Convert(catalogContentBase);

            _entries.Add(entry);

            return null;
        }

        public override byte[] SerializeEntry(object value, CancellationToken cancellationToken)
        {
            return Array.Empty<byte>();
        }
    }
}
