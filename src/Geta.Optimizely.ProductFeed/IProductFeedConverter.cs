// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using EPiServer.Web;

namespace Geta.Optimizely.ProductFeed
{
    public interface IProductFeedConverter<in TEntity>
    {
        object Convert(TEntity entity, HostDefinition host);
    }
}
