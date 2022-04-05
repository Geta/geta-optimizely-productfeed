// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Google.Models;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed.Google
{
    public class GoogleProductFeedConverter : IProductFeedContentConverter
    {
        private readonly Func<Type, IProductFeedEntityMapper> _mapperFactory;

        public GoogleProductFeedConverter(Func<Type, IProductFeedEntityMapper> mapperFactory)
        {
            _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));
        }

        public ICollection<FeedEntity> Convert(
            ICollection<CatalogContentBase> sourceData,
            FeedDescriptor feedDescriptor,
            CancellationToken cancellationToken)
        {
            var generatedFeeds = new List<Feed>();
            var mapper = _mapperFactory(feedDescriptor.Mapper);

            var feedEntity = mapper.GenerateFeedEntity(feedDescriptor);

            if (feedEntity == null)
            {
                return new List<FeedEntity>();
            }

            feedEntity.Entries = sourceData
                .Select(sd => mapper.GenerateEntry(sd))
                .Where(sd => sd != null)
                .ToList();

            generatedFeeds.Add(feedEntity);

            return generatedFeeds
                .Select(f => new FeedEntity
                {
                    CreatedUtc = f.Updated,
                    Link = f.Link,
                    FeedBytes = ObjectXmlSerializer.Serialize(f, typeof(Feed))
                }).ToList();
        }
    }
}
