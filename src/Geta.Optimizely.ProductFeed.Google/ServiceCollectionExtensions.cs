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
            services.AddTransient<GoogleProductFeedExporter>();

            var descriptor = new GoogleFeedDescriptor();
            setupAction(descriptor);

            if (descriptor.Converter == null)
            {
                throw new InvalidOperationException("Google ProductFeed mapper is not set. Use `GoogleFeedDescriptor.SetExporter<T>` method to do so.");
            }

            services.AddTransient(descriptor.Converter);
            services.AddSingleton<FeedDescriptor>(descriptor);

            return services;
        }
    }
}
