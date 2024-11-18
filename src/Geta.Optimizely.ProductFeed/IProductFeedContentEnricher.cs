// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Threading;

namespace Geta.Optimizely.ProductFeed;

public interface IProductFeedContentEnricher<T>
{
    T Enrich(T sourceData, CancellationToken cancellationToken);
}

internal class DefaultIProductFeedContentEnricher<TEntity> : IProductFeedContentEnricher<TEntity>
{
    public TEntity Enrich(TEntity sourceData, CancellationToken cancellationToken)
    {
        return sourceData;
    }
}
