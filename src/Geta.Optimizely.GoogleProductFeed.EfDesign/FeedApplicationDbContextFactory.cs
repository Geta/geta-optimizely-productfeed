using System;
using System.IO;
using Geta.Optimizely.GoogleProductFeed.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Geta.Optimizely.GoogleProductFeed.EfDesign
{
    public class FeedApplicationDbContextFactory : IDesignTimeDbContextFactory<FeedApplicationDbContext>
    {
        public FeedApplicationDbContext CreateDbContext(string[] args)
        {
            var connectionString = GetConnectionString();
            var optionsBuilder = new DbContextOptionsBuilder<FeedApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new FeedApplicationDbContext(optionsBuilder.Options);
        }

        private static string GetConnectionString()
        {
            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{envName}.json", optional: true)
                .Build();

            return configuration.GetConnectionString("GoogleProductFeed");
        }
    }
}