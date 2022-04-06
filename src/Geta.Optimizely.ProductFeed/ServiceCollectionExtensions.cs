// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
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

        public static IServiceCollection AddProductFeed(this IServiceCollection services, Action<ProductFeedOptions> setupAction)
        {
            services.AddTransient<IProductFeedContentLoader, DefaultProductFeedContentLoader>();
            services.AddTransient<IProductFeedContentEnricher, DefaultIProductFeedContentEnricher>();

            services.AddSingleton<Func<FeedDescriptor, IProductFeedContentExporter>>(
                provider => d =>
                {
                    var exporter = provider.GetRequiredService(d.Exporter) as AbstractFeedContentExporter;
                    var converter = provider.GetRequiredService(d.Converter) as IProductFeedConverter;

                    if (exporter != null && converter != null)
                    {
                        exporter.SetDescriptor(d);
                        exporter.SetConverter(converter);
                    }

                    return exporter;
                });

            services.AddTransient<IFeedRepository, FeedRepository>();
            services.AddTransient(provider =>
                                      new FeedApplicationDbContext(provider.GetRequiredService<IOptions<ProductFeedOptions>>()));

            services.AddTransient<FeedBuilderCreateJob>();
            services.AddHostedService<MigrationService>();

            var config = new ProductFeedOptions();
            setupAction(config);

            foreach (var descriptor in config.Descriptors)
            {
                services.AddSingleton(descriptor);
                services.AddTransient(descriptor.Converter);
                services.AddTransient(descriptor.Exporter);
            }

            services.AddOptions<ProductFeedOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    setupAction(options);
                    configuration.GetSection("Geta:ProductFeed").Bind(options);
                });

            return services;
        }
    }
}
