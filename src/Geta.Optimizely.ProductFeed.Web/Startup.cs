using EPiServer.Web.Routing;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Csv;
using Geta.Optimizely.ProductFeed.Google;
using Geta.Optimizely.ProductFeed.Web.Converters;
using Geta.Optimizely.ProductFeed.Web.Enrichers;
using Geta.Optimizely.ProductFeed.Web.Exporters;
using Geta.Optimizely.ProductFeed.Web.Mappers;
using Geta.Optimizely.ProductFeed.Web.Models;

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

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        _foundationStartup.Configure(app,
                                     env,
                                     endpoints =>
                                     {
                                         endpoints.MapContent();
                                         endpoints.MapRazorPages();
                                         endpoints.MapProductFeeds();
                                     });
    }
}
