// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using Geta.Optimizely.GoogleProductFeed.Configuration;
using Geta.Optimizely.GoogleProductFeed.Models;
using Microsoft.EntityFrameworkCore;

namespace Geta.Optimizely.GoogleProductFeed.Repositories
{
    public class FeedApplicationDbContext : DbContext
    {
        private readonly string _connectionString;

        public FeedApplicationDbContext(DbSettings settings)
        {
            _connectionString = settings.ConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (options.IsConfigured) return;

            options.UseSqlServer(_connectionString);
        }

        public DbSet<FeedData> FeedData { get; set; }
    }
}
