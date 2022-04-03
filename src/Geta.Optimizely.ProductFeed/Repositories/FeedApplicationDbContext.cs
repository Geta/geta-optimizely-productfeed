// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Geta.Optimizely.GoogleProductFeed.Repositories
{
    public class FeedApplicationDbContext : DbContext
    {
        private readonly string _connectionString;

        public FeedApplicationDbContext(IOptions<ProductFeedOptions> options)
        {
            _connectionString = options.Value.ConnectionString;
        }

        public FeedApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (options.IsConfigured) return;

            options.UseSqlServer(_connectionString);
        }

        public DbSet<FeedEntity> FeedData { get; set; }
    }
}
