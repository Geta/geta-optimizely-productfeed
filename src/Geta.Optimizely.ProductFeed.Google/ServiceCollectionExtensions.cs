// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using Microsoft.Extensions.DependencyInjection;

namespace Geta.Optimizely.ProductFeed.Google
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGoogleProductFeed(this IServiceCollection services)
        {
            return services;
        }
    }
}
