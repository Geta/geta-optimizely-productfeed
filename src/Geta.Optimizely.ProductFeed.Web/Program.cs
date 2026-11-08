using Geta.Optimizely.ProductFeed.Web;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;

Host.CreateDefaultBuilder(args)
    .ConfigureCmsDefaults()
    .ConfigureAppConfiguration((_, config) => UseSandboxGraphSettings(config))
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
        webBuilder.UseContentRoot(Path.GetFullPath("../../sub/geta-foundation-core/src/Foundation"));
    })
    .Build()
    .Run();

// The content root is the Foundation project, so its appsettings.json (including its own Optimizely Graph keys) is
// loaded instead of this project's. Layer this project's Optimizely:ContentGraph section right on top of that file, so
// the sandbox runs without Graph credentials by default. appsettings.{Environment}.json (where Foundation says to put
// real keys), environment variables and the command line still take precedence.
static void UseSandboxGraphSettings(IConfigurationBuilder config)
{
    var graphSettings = new ConfigurationBuilder()
        .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: true)
        .Build()
        .GetSection("Optimizely:ContentGraph")
        .AsEnumerable()
        .Where(setting => setting.Value != null);

    var sources = config.Sources.ToList();
    var appSettings = sources.FindIndex(source => source is JsonConfigurationSource { Path: "appsettings.json" });
    if (appSettings < 0)
    {
        appSettings = sources.FindLastIndex(source => source is JsonConfigurationSource);
    }

    config.Sources.Insert(appSettings + 1, new MemoryConfigurationSource { InitialData = graphSettings });
}
