// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Configuration;

namespace Geta.Optimizely.ProductFeed.Csv
{
    public static class ProductFeedOptionsExtensions
    {
        public static ProductFeedOptions<TEntity> AddCsvExport<TEntity>(
            this ProductFeedOptions<TEntity> options,
            Action<CsvFeedDescriptor<TEntity>> setupAction)
        {
            var descriptor = new CsvFeedDescriptor<TEntity>();

            descriptor.SetExporter<CsvExporter<TEntity>, TEntity>();
            descriptor.SetSiteUrlBuilder<DefaultSiteUrlBuilder>();

            setupAction(descriptor);
            options.Add(descriptor);

            return options;
        }
    }
}
