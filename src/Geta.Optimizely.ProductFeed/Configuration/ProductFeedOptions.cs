// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;

namespace Geta.Optimizely.ProductFeed.Configuration
{
    public class ProductFeedOptions<TEntity>
    {
        public string ConnectionString { get; set; }

        public void Add(FeedDescriptor feedDescriptor)
        {
            Descriptors.Add(feedDescriptor);
        }

        public List<FeedDescriptor> Descriptors { get; set; } = new();
    }
}
