// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using Geta.Optimizely.ProductFeed.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Geta.Optimizely.ProductFeed.Google
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGoogleProductFeed(
            this IServiceCollection services,
            Action<GoogleFeedDescriptor> setupAction)
        {
            services.AddTransient<GoogleProductFeedConverter>();

            var descriptor = new GoogleFeedDescriptor();
            setupAction(descriptor);

            services.AddSingleton<Func<Type, IProductFeedEntityMapper>>(
                provider => t => provider.GetRequiredService(t) as IProductFeedEntityMapper);

            // register mappers
            if (descriptor.Mapper != null)
            {
                services.AddTransient(descriptor.Mapper);
            }

            services.AddSingleton<FeedDescriptor>(descriptor);
            services.AddSingleton(descriptor);

            return services;
        }
    }
}
