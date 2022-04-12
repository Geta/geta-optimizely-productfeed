// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;

namespace Geta.Optimizely.ProductFeed.Configuration
{
    public class ProductFeedOptions
    {
        public Type MappedEntity { get; set; }
    }

    public class ProductFeedOptions<TEntity>
    {
        public string ConnectionString { get; set; }

        public List<FeedDescriptor> Descriptors { get; set; } = new();

        public Type EntityMapper { get; set; }

        public void Add(FeedDescriptor feedDescriptor)
        {
            Descriptors.Add(feedDescriptor);
        }

        public ProductFeedOptions<TEntity> SetEntityMapper<TEntityMapper>() where TEntityMapper : IEntityMapper<TEntity>
        {
            EntityMapper = typeof(TEntityMapper);
            return this;
        }
    }
}
