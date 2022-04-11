// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.GoogleProductFeed.Repositories;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Geta.Optimizely.ProductFeed
{

    public delegate object ServiceFactory(Type serviceType);

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddProductFeed<TEntity>(
            this IServiceCollection services,
            Action<ProductFeedOptions<TEntity>> setupAction)
        {
            services.AddTransient<ServiceFactory>(sp => sp.GetService);

            services.AddTransient(typeof(IProductFeedContentLoader<TEntity>), typeof(DefaultProductFeedContentLoader));
            services.AddTransient(typeof(IProductFeedContentEnricher<TEntity>), typeof(DefaultIProductFeedContentEnricher));
            services.AddTransient(typeof(ProcessingPipeline<TEntity>), typeof(ProcessingPipeline<TEntity>));

            services.AddTransient<IFeedRepository, FeedRepository>();
            services.AddTransient(provider =>
            {
                var options = provider.GetRequiredService<IOptions<ProductFeedOptions<TEntity>>>();
                return new FeedApplicationDbContext(options.Value.ConnectionString);
            });

            services.AddTransient<FeedBuilderCreateJob>();
            services.AddHostedService<MigrationService>();

            services.AddSingleton<Func<FeedDescriptor, AbstractFeedContentExporter<TEntity>>>(
                provider => d =>
                {
                    var exporter = provider.GetRequiredService(d.Exporter) as AbstractFeedContentExporter<TEntity>;
                    var converter = provider.GetRequiredService(d.Converter) as IProductFeedConverter<TEntity>;
                    var siteUrlBuilder = provider.GetRequiredService(d.SiteUrlBuilder) as ISiteUrlBuilder;

                    if (exporter != null && converter != null && siteUrlBuilder != null)
                    {
                        exporter.SetDescriptor(d);
                        exporter.SetConverter(converter);
                        exporter.SetSiteUrlBuilder(siteUrlBuilder);
                    }

                    return exporter;
                });

            var config = new ProductFeedOptions<TEntity>();
            setupAction(config);

            foreach (var descriptor in config.Descriptors)
            {
                services.AddSingleton(descriptor);
                services.AddTransient(descriptor.Converter);
                services.AddTransient(descriptor.Exporter);
                services.AddTransient(descriptor.SiteUrlBuilder);
            }

            services.AddOptions<ProductFeedOptions<TEntity>>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    setupAction(options);
                    configuration.GetSection("Geta:ProductFeed").Bind(options);
                });

            return services;
        }
    }
}
