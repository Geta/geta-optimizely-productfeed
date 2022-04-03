// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Google.Models;
using Geta.Optimizely.ProductFeed.Models;
using Microsoft.Extensions.Logging;

namespace Geta.Optimizely.ProductFeed.Google
{
    public class GoogleProductFeedConverter : IProductFeedContentConverter
    {
        private readonly GoogleFeedDescriptor _descriptor;
        private readonly Func<Type, IProductFeedEntityMapper> _mapperFactory;
        private readonly ILogger<GoogleProductFeedConverter> _logger;

        public GoogleProductFeedConverter(
            GoogleFeedDescriptor descriptor,
            Func<Type, IProductFeedEntityMapper> mapperFactory,
            ILogger<GoogleProductFeedConverter> logger)
        {
            _descriptor = descriptor;
            _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));
            _logger = logger;
        }

        public ICollection<FeedEntity> Convert(
            ICollection<CatalogContentBase> sourceData,
            FeedDescriptor feedDescriptor)
        {
            var generatedFeedsData = new List<FeedEntity>();
            var generatedFeeds = new List<Feed>();
            var entries = new List<Entry>();
            var mapper = _mapperFactory(_descriptor.Mapper);

            var feedEntity = mapper.GenerateFeedEntity(feedDescriptor);

            if (feedEntity == null)
            {
                return new List<FeedEntity>();
            }


            foreach (var catalogContent in sourceData)
            {
                try
                {
                    var entry = mapper.GenerateEntry(catalogContent);

                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to generate GoogleProductFeed entry for ContentGuid={catalogContent.ContentGuid}");
                }
            }

            feedEntity.Entries = entries;
            generatedFeeds.Add(feedEntity);


            foreach (var generatedFeed in generatedFeeds)
            {
                var feedData = new FeedEntity
                {
                    CreatedUtc = generatedFeed.Updated,
                    Link = generatedFeed.Link,
                    FeedBytes = ObjectXmlSerializer.Serialize(generatedFeed, typeof(Feed))
                };

                generatedFeedsData.Add(feedData);
            }

            return generatedFeedsData;
        }
    }
}
