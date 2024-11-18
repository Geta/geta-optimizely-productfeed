// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using Geta.Optimizely.ProductFeed.Configuration;

namespace Geta.Optimizely.ProductFeed.Google;

public static class ProductFeedOptionsExtensions
{
    public static ProductFeedOptions<TEntity> AddGoogleXmlExport<TEntity>(
        this ProductFeedOptions<TEntity> options,
        Action<GoogleFeedDescriptor<TEntity>> setupAction)
    {
        var descriptor = new GoogleFeedDescriptor<TEntity>();

        descriptor.SetExporter<GoogleFeedExporter<TEntity>, TEntity>();
        descriptor.SetSiteUrlBuilder<DefaultSiteUrlBuilder>();

        setupAction(descriptor);
        options.Add(descriptor);

        return options;
    }
}
