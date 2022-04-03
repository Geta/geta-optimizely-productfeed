// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Repositories;
using Microsoft.Extensions.Logging;

namespace Geta.Optimizely.ProductFeed
{
    public class ProductFeedBuilder : IProductFeedBuilder
    {
        private readonly IProductFeedContentLoader _feedContentLoader;
        private readonly IEnumerable<IProductFeedContentEnricher> _enrichers;
        private readonly IEnumerable<FeedDescriptor> _feedDescriptors;
        private readonly Func<Type, IProductFeedContentConverter> _converterFactory;
        private readonly IFeedRepository _feedRepository;
        private readonly ILogger<ProductFeedBuilder> _logger;

        public ProductFeedBuilder(
            IProductFeedContentLoader feedContentLoader,
            IEnumerable<IProductFeedContentEnricher> enrichers,
            IEnumerable<FeedDescriptor> feedDescriptors,
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
            try
            {
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
                            var result = converter.Convert(feedSourceData, feedDescriptor);

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
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to execute feed builder.");
                return false;
            }
        }
    }
}
