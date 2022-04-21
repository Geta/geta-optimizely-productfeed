// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using Geta.Optimizely.ProductFeed.Models;
using Microsoft.EntityFrameworkCore;

// TODO: test namespace change
namespace Geta.Optimizely.GoogleProductFeed.Repositories
{
    public class FeedApplicationDbContext : DbContext
    {
        private readonly string _connectionString;

        public FeedApplicationDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public FeedApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;

            optionsBuilder.UseSqlServer(_connectionString);
        }

        public DbSet<FeedEntity> FeedData { get; set; }
    }
}
