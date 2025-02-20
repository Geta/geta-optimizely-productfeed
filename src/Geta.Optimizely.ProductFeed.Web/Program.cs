using Geta.Optimizely.ProductFeed.Web;

Host.CreateDefaultBuilder(args)
    .ConfigureCmsDefaults()
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
        webBuilder.UseContentRoot(Path.GetFullPath("../../sub/geta-foundation-core/src/Foundation"));
    })
    .Build()
    .Run();
