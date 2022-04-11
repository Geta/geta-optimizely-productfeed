// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed
{
    public abstract class AbstractFeedContentExporter<TEntity>
    {
        protected readonly MemoryStream Buffer = new();

        public IProductFeedConverter<TEntity> Converter { get; private set; }

        public ISiteUrlBuilder SiteUrlBuilder { get; private set; }

        public FeedDescriptor Descriptor { get; private set; }

        public virtual void BeginExport(CancellationToken cancellationToken) { }

        public virtual void BuildEntry(TEntity catalogContentBase, CancellationToken cancellationToken)
        {
            var entry = ConvertEntry(catalogContentBase, cancellationToken);
            if (entry == null)
            {
                return;
            }

            var serialized = SerializeEntry(entry, cancellationToken);
            if (serialized != null)
            {
                Buffer.Write(serialized);
            }
        }

        public abstract byte[] SerializeEntry(object value, CancellationToken cancellationToken);

        public abstract object ConvertEntry(TEntity catalogContentBase, CancellationToken cancellationToken);

        public virtual ICollection<FeedEntity> FinishExport(CancellationToken cancellationToken)
        {
            return new[]
            {
                new FeedEntity
                {
                    CreatedUtc = DateTime.UtcNow,
                    Link = $"{SiteUrlBuilder.BuildUrl().TrimEnd('/')}/{Descriptor.FileName.TrimStart('/')}",
                    FeedBytes = Buffer.ToArray()
                }
            };
        }

        public void SetConverter(IProductFeedConverter<TEntity> converter)
        {
            Converter = converter;
        }

        public void SetSiteUrlBuilder(ISiteUrlBuilder urlBuilder)
        {
            SiteUrlBuilder = urlBuilder;
        }

        public void SetDescriptor(FeedDescriptor feedDescriptor)
        {
            Descriptor = feedDescriptor;
        }
    }
}
