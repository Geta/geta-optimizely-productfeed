// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Geta.Optimizely.GoogleProductFeed;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;
using Geta.Optimizely.ProductFeed.Repositories;
using Microsoft.Extensions.Logging;

namespace Geta.Optimizely.ProductFeed
{
    public class ProductFeedBuilder : IProductFeedBuilder
    {
        private readonly IProductFeedContentLoader _feedContentLoader;
        private readonly IEnumerable<IProductFeedContentEnricher> _enrichers;
        private readonly IEnumerable<ProductFeedDescriptor> _feedDescriptors;
        private readonly Func<Type, IProductFeedContentConverter> _converterFactory;
        private readonly IFeedRepository _feedRepository;
        private readonly ILogger<ProductFeedBuilder> _logger;

        private const string Ns = "http://www.w3.org/2005/Atom";

        public ProductFeedBuilder(
            IProductFeedContentLoader feedContentLoader,
            IEnumerable<IProductFeedContentEnricher> enrichers,
            IEnumerable<ProductFeedDescriptor> feedDescriptors,
            Func<Type, IProductFeedContentConverter> converterFactory,
            IFeedRepository feedRepository,
            ILogger<ProductFeedBuilder> logger)
        {
            _feedContentLoader = feedContentLoader;
            _enrichers = enrichers;
            _feedDescriptors = feedDescriptors;
            _converterFactory = converterFactory;
            _feedRepository = feedRepository;
            _logger = logger;
        }

        public bool Build()
        {
            // load
            var feedSourceData = _feedContentLoader.LoadSourceData();

            // enrich
            foreach (var enricher in _enrichers)
            {
                enricher.Enrich(feedSourceData);
            }

            foreach (var feedDescriptor in _feedDescriptors)
            {
                try
                {
                    // convert
                    var converter = _converterFactory(feedDescriptor.Converter);
                    if (converter != null)
                    {
                        var result = converter.Convert(feedSourceData);

                        // save
                        _feedRepository.Save(result);
                    }
                    else
                    {
                        _logger.LogWarning($"Could not found converter for `{feedDescriptor.Name}` feed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to build `{feedDescriptor.Name}` product feed");
                }
            }

            return true;
        }

        public Feed GetLatestFeed(string siteHost)
        {
            FeedData feedData = _feedRepository.GetLatestFeedData(siteHost);

            if (feedData == null)
            {
                return null;
            }

            var serializer = new XmlSerializer(typeof(Feed), Ns);
            using (var ms = new MemoryStream(feedData.FeedBytes))
            {
                return serializer.Deserialize(ms) as Feed;
            }
        }
    }


}
