// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

namespace Geta.Optimizely.ProductFeed.Configuration;

public static class FeedDescriptorExtensions
{
    public static FeedDescriptor SetExporter<TExporter, TEntity>(this FeedDescriptor descriptor)
        where TExporter : AbstractFeedContentExporter<TEntity>
    {
        descriptor.Exporter = typeof(TExporter);
        return descriptor;
    }
}
