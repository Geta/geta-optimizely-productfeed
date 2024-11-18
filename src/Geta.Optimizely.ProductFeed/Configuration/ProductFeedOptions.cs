// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Geta.Optimizely.ProductFeed.Configuration
{
    public class ProductFeedOptions
    {
        public Type SiteBuilder { get; set; }

        public Type MappedEntity { get; set; }
    }

    public class ProductFeedOptions<TEntity> : ProductFeedOptions
    {
        public string ConnectionString { get; set; }

        public List<FeedDescriptor> Descriptors { get; set; } = new();

        public List<Type> Enrichers { get; set; } = new();

        public Type Filter { get; set; }

        public Type EntityMapper { get; set; }

        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(30);

        public void Add(FeedDescriptor feedDescriptor)
        {
            Descriptors.Add(feedDescriptor);
        }

        public ProductFeedOptions<TEntity> SetEntityMapper<TEntityMapper>() where TEntityMapper : IEntityMapper<TEntity>
        {
            EntityMapper = typeof(TEntityMapper);
            return this;
        }

        public ProductFeedOptions<TEntity> AddEnricher<TEnricher>() where TEnricher : IProductFeedContentEnricher<TEntity>
        {
            Enrichers.Add(typeof(TEnricher));
            return this;
        }

        public ProductFeedOptions<TEntity> SetFilter<TFilter>() where TFilter : IProductFeedFilter<TEntity>
        {
            Filter = typeof(TFilter);
            return this;
        }

        public ProductFeedOptions<TEntity> SetSiteBuilder<TBuilder>() where TBuilder : ISiteBuilder
        {
            SiteBuilder = typeof(TBuilder);
            return this;
        }
    }
}
