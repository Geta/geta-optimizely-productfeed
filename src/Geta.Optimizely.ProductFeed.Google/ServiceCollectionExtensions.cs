// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using Geta.Optimizely.ProductFeed.Configuration;
using Lucene.Net.Util.Automaton;
using Microsoft.Extensions.DependencyInjection;

namespace Geta.Optimizely.ProductFeed.Google
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGoogleProductFeed(
            this IServiceCollection services,
            Action<GoogleProductFeedDescriptor> setupAction)
        {
            services.AddTransient<GoogleProductFeedConverter>();

            var descriptor = new GoogleProductFeedDescriptor();
            setupAction(descriptor);

            // register mappers
            if (descriptor.Mapper != null)
            {
                services.AddTransient(descriptor.Mapper);
            }

            services.AddSingleton<ProductFeedDescriptor>(descriptor);
            services.AddSingleton(descriptor);

            return services;
        }
    }
}
