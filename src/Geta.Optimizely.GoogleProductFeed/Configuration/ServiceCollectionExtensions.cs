// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using Geta.Optimizely.GoogleProductFeed.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Geta.Optimizely.GoogleProductFeed.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGoogleProductFeed(
            this IServiceCollection services,
            Action<GoogleProductFeedOptions> setupAction)
        {
            services.AddTransient<IFeedHelper, FeedHelper>();
            services.AddTransient<IFeedRepository, FeedRepository>();
            services.AddTransient<FeedApplicationDbContext>();
            services.AddTransient<FeedBuilderCreateJob>();

            services.AddOptions<GoogleProductFeedOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                setupAction(options);
                configuration.GetSection("Geta:GoogleProductFeed").Bind(options);
            });

            return services;
        }
    }
}