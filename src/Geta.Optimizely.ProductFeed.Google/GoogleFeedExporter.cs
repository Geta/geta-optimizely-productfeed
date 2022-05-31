// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EPiServer.Web;
using Geta.Optimizely.ProductFeed.Google.Models;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed.Google
{
    public class GoogleFeedExporter<TEntity> : AbstractFeedContentExporter<TEntity>
    {
        private readonly List<Entry> _entries = new();

        public override void BeginExport(HostDefinition host, CancellationToken cancellationToken)
        {
            _entries.Clear();
            base.BeginExport(host, cancellationToken);
        }

        public override ICollection<FeedEntity> FinishExport(HostDefinition host, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var f = new Feed
            {
                Updated = DateTime.UtcNow,
                Title = "Google Product Feed",
                Link = host?.Url.ToString().TrimEnd('/') + '/' + Descriptor.FileName.TrimStart('/'),
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

        public override object ConvertEntry(TEntity entity, HostDefinition host, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = (Entry)Converter.Convert(entity, host);

            _entries.Add(entry);

            return null;
        }

        public override byte[] SerializeEntry(object value, CancellationToken cancellationToken)
        {
            return Array.Empty<byte>();
        }
    }
}
