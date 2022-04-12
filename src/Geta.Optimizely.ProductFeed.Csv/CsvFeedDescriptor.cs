// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using Geta.Optimizely.ProductFeed.Configuration;

namespace Geta.Optimizely.ProductFeed.Csv
{
    public class CsvFeedDescriptor<TEntity> : FeedDescriptor
    {
        public CsvFeedDescriptor() : base("csv", "csv-feed", "text/plain") { }

        public CsvFeedDescriptor<TEntity> SetConverter<TConverter>() where TConverter : IProductFeedConverter<TEntity>
        {
            Converter = typeof(TConverter);
            return this;
        }
    }
}