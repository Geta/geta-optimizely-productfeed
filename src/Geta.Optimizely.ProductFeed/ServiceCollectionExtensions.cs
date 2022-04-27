// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
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

            services.AddTransient<IProductFeedContentLoader, DefaultProductFeedContentLoader>();
            services.AddTransient(typeof(IProductFeedContentEnricher<TEntity>), typeof(DefaultIProductFeedContentEnricher<TEntity>));
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

                    if (exporter != null)
                    {
                        if (d.Filter != null && provider.GetService(d.Filter) is IProductFeedFilter<TEntity> filter)
                        {
                            exporter.SetFilter(filter);
                        }

                        var converter = provider.GetRequiredService(d.Converter) as IProductFeedConverter<TEntity>;
                        var siteUrlBuilder = provider.GetRequiredService(d.SiteUrlBuilder) as ISiteUrlBuilder;

                        if (converter != null && siteUrlBuilder != null)
                        {
                            exporter.SetDescriptor(d);
                            exporter.SetConverter(converter);
                            exporter.SetSiteUrlBuilder(siteUrlBuilder);
                        }
                    }

                    return exporter;
                });

            var generalOptions = new ProductFeedOptions { MappedEntity = typeof(TEntity) };
            services.AddSingleton(generalOptions);

            var config = new ProductFeedOptions<TEntity>();
            setupAction(config);

            if (config.EntityMapper == null)
            {
                if (!typeof(TEntity).IsAssignableFrom(typeof(CatalogContentBase)))
                {
                    throw new InvalidOperationException("Entity mapper is not set");
                }

                services.AddTransient<IEntityMapper<CatalogContentBase>, DefaultEntityMapper>();
            }
            else
            {
                services.AddTransient(typeof(IEntityMapper<TEntity>), config.EntityMapper);
            }

            if (config.Filter != null)
            {
                services.AddTransient(typeof(IProductFeedFilter<TEntity>), config.Filter);
            }

            foreach (var enricher in config.Enrichers)
            {
                services.AddTransient(typeof(IProductFeedContentEnricher<TEntity>), enricher);
            }

            foreach (var descriptor in config.Descriptors)
            {
                // adding descriptor as FeedDescriptor - for the pipeline to find all registered feeds
                services.AddSingleton(descriptor);

                // adding as actual underlying type - for specific type injections (for example when Csv exporter needs CsvFeedDescriptor)
                services.AddSingleton(descriptor.GetType(), descriptor);

                if (descriptor.Filter != null)
                {
                    services.AddTransient(descriptor.Filter);
                }

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
