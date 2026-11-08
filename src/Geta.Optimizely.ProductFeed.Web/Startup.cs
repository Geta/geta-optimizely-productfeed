using System.Reflection;
using EPiServer.Web.Routing;
using Geta.Optimizely.ProductFeed.Web.Services;
using Optimizely.Graph.Cms.Configuration;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Csv;
using Geta.Optimizely.ProductFeed.Google;
using Geta.Optimizely.ProductFeed.Web.Converters;
using Geta.Optimizely.ProductFeed.Web.Enrichers;
using Geta.Optimizely.ProductFeed.Web.Exporters;
using Geta.Optimizely.ProductFeed.Web.Mappers;
using Geta.Optimizely.ProductFeed.Web.Models;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Geta.Optimizely.ProductFeed.Web;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly Foundation.Startup _foundationStartup;

    public Startup(IWebHostEnvironment webHostingEnvironment, IConfiguration configuration)
    {
        _configuration = configuration;
        _foundationStartup = new Foundation.Startup(webHostingEnvironment, configuration);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        _foundationStartup.ConfigureServices(services);
        UseNoOpGraphSyncClientWhenUnconfigured(services);

        services
            .AddProductFeed<MyCommerceProductRecord>(options =>
            {
                options.ConnectionString = _configuration.GetConnectionString("EPiServerDB");
                options.CommandTimeout = TimeSpan.FromMinutes(1);

                options.SetEntityMapper<EntityMapper>();

                //options.SetFilter<GenericEntityFilter>();
                options.AddEnricher<FashionProductAvailabilityEnricher>();

                //options.SetSiteBuilder<MySiteBuilder>();

                options.AddGoogleXmlExport(d =>
                {
                    d.FileName = "/google-feed";
                    //d.SetFilter<GoogleXmlFilter>();
                    d.SetConverter<GoogleXmlConverter>();
                });

                options.AddCsvExport(d =>
                {
                    d.FileName = "/csv-feed-1";
                    d.SetConverter<CsvConverter>();
                    d.CsvEntityType = typeof(CsvEntry);
                });
            });
    }

    // When no ContentGraph AppKey is configured (the sandbox default), turn off Graph indexing and log sending,
    // which would otherwise call Graph with empty credentials, and replace the Graph sync client with a no-op so
    // startup doesn't fail on Graph schema initialization (it checks the schema regardless of IndexingEnabled).
    // Done in code rather than appsettings.json so real keys in appsettings.{Environment}.json still enable Graph.
    private void UseNoOpGraphSyncClientWhenUnconfigured(IServiceCollection services)
    {
        if (!string.IsNullOrEmpty(_configuration["Optimizely:ContentGraph:AppKey"]))
        {
            return;
        }

        services.PostConfigure<GraphCmsOptions>(options =>
        {
            options.IndexingEnabled = false;
            options.EventIndexingEnabled = false;
        });
        services.PostConfigure<GraphLoggingOptions>(options => options.AllowSendingLogs = false);

        // ISyncClient is internal to Optimizely.Graph.Cms, so it can only be found by name.
        var syncClientType = typeof(GraphCmsOptions).Assembly.GetType("Optimizely.Graph.Cms.Client.ISyncClient");
        if (syncClientType == null)
        {
            return;
        }

        services.Replace(ServiceDescriptor.Singleton(syncClientType, DispatchProxy.Create(syncClientType, typeof(NoOpSyncClientProxy))));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Foundation.Configure now sets up routing and maps content/controllers/razor pages itself
        // and calls UseEndpoints internally (the endpoints callback overload was removed in the CMS 13
        // upgrade). Calling app.UseEndpoints() a second time here would re-register every Foundation
        // endpoint data source and throw "Duplicate endpoint name". Instead we reuse the endpoint route
        // builder Foundation already created to add the product feed routes to the same data source.
        _foundationStartup.Configure(app, env);

        const string EndpointRouteBuilderKey = "__EndpointRouteBuilder";

        if (app.Properties.TryGetValue(EndpointRouteBuilderKey, out var value)
            && value is IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapProductFeeds();
        }
        else
        {
            throw new InvalidOperationException(
                $"Unable to find '{EndpointRouteBuilderKey}' in ApplicationBuilder properties. " +
                "Product feeds could not be mapped. Ensure UseRouting/UseEndpoints has run (Foundation.Configure) before this call.");
        }
    }
}
