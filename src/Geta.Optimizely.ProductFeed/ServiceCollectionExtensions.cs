// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using Geta.Optimizely.GoogleProductFeed;
using Geta.Optimizely.GoogleProductFeed.Repositories;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Geta.Optimizely.ProductFeed
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProductFeed(
            this IServiceCollection services)
        {
            return services.AddProductFeed(_ => { });
        }

        public static IServiceCollection AddProductFeed(
            this IServiceCollection services,
            Action<ProductFeedOptions> setupAction)
        {
            services.AddControllers()
                .AddXmlSerializerFormatters();

            services.AddTransient<IFeedHelper, FeedHelper>();
            services.AddTransient<IFeedRepository, FeedRepository>();
            services.AddTransient(provider =>
                new FeedApplicationDbContext(provider.GetRequiredService<IOptions<ProductFeedOptions>>()));

            services.AddTransient<FeedBuilderCreateJob>();

            services.AddHostedService<MigrationService>();

            services.AddOptions<ProductFeedOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                setupAction(options);
                configuration.GetSection("Geta:GoogleProductFeed").Bind(options);
            });

            return services;
        }
    }
}