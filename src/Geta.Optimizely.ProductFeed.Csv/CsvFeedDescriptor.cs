// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using Geta.Optimizely.ProductFeed.Configuration;

namespace Geta.Optimizely.ProductFeed.Csv;

public class CsvFeedDescriptor<TEntity> : FeedDescriptor
{
    public CsvFeedDescriptor() : base("csv", "csv-feed", "text/plain") { }

    public Type CsvEntityType { get; set; }

    public CsvFeedDescriptor<TEntity> SetConverter<TConverter>() where TConverter : IProductFeedConverter<TEntity>
    {
        Converter = typeof(TConverter);
        return this;
    }

    public CsvFeedDescriptor<TEntity> SetFilter<TFilter>() where TFilter : IProductFeedFilter<TEntity>
    {
        Filter = typeof(TFilter);
        return this;
    }
}
