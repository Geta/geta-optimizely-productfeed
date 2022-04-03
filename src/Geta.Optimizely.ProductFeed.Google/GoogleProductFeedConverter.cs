// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Models;
using Microsoft.Extensions.Logging;

namespace Geta.Optimizely.ProductFeed.Google
{
    public class GoogleProductFeedConverter : IProductFeedContentConverter
    {
        private readonly GoogleProductFeedDescriptor _descriptor;
        private readonly Func<Type, IProductFeedEntityMapper> _mapperFactory;
        private readonly ILogger<GoogleProductFeedConverter> _logger;
        private const string Ns = "http://www.w3.org/2005/Atom";

        public GoogleProductFeedConverter(
            GoogleProductFeedDescriptor descriptor,
            Func<Type, IProductFeedEntityMapper> mapperFactory,
            ILogger<GoogleProductFeedConverter> logger)
        {
            _descriptor = descriptor;
            _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));
            _logger = logger;
        }

        public ICollection<FeedData> Convert(ICollection<CatalogContentBase> sourceData)
        {
            var generatedFeedsData = new List<FeedData>();
            var generatedFeeds = new List<Feed>();
            var entries = new List<Entry>();
            var mapper = _mapperFactory(_descriptor.Mapper);

            var feedEntity = mapper.GenerateFeedEntity();

            if (feedEntity == null)
            {
                return new List<FeedData>();
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
                var feedData = new FeedData
                {
                    CreatedUtc = generatedFeed.Updated,
                    Link = generatedFeed.Link
                };

                using (var ms = new MemoryStream())
                {
                    var serializer = new XmlSerializer(typeof(Feed), Ns);
                    serializer.Serialize(ms, generatedFeed);
                    feedData.FeedBytes = ms.ToArray();
                }

                generatedFeedsData.Add(feedData);
            }

            return generatedFeedsData;
        }
    }
}
