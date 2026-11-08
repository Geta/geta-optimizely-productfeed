// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Geta.Optimizely.GoogleProductFeed.Repositories;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;
using Geta.Optimizely.ProductFeed.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Geta.Optimizely.ProductFeed.Tests;

public class FeedRepositoryTests
{
    private readonly FeedApplicationDbContext _dbContext = new(
        new DbContextOptionsBuilder<FeedApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private readonly FeedRepository _repository;

    public FeedRepositoryTests()
    {
        _repository = new FeedRepository(_dbContext, [new FeedDescriptor("google", "/google-feed", "application/xml")]);
    }

    [Theory]
    [InlineData("https://example.com/google-feed")]
    [InlineData("https://EXAMPLE.com/google-feed?utm_source=x")]
    [InlineData("https://example.com/google-feed/")]
    public void GetLatestFeed_MatchesStoredLink(string requestUrl)
    {
        _repository.Save([Feed("https://example.com/google-feed", "content")]);

        var feed = _repository.GetLatestFeed(new Uri(requestUrl));

        Assert.NotNull(feed);
        Assert.Equal("content", Encoding.UTF8.GetString(feed.FeedBytes));
    }

    [Fact]
    public void GetLatestFeed_InternationalizedDomainName_MatchesEncodedRequestHost()
    {
        // Links are stored from HostDefinition.Url (Unicode host); requests carry the IDN-encoded host (issue #14).
        _repository.Save([Feed("https://bücher.de/google-feed", "idn")]);

        var feed = _repository.GetLatestFeed(new Uri("https://xn--bcher-kva.de/google-feed"));

        Assert.NotNull(feed);
        Assert.Equal("idn", Encoding.UTF8.GetString(feed.FeedBytes));
    }

    [Fact]
    public void GetLatestFeed_UnknownLink_ReturnsNull()
    {
        _repository.Save([Feed("https://example.com/google-feed", "content")]);

        Assert.Null(_repository.GetLatestFeed(new Uri("https://other.com/google-feed")));
    }

    [Fact]
    public void Save_ExistingLink_UpdatesFeedInsteadOfAddingRow()
    {
        _repository.Save([Feed("https://bücher.de/google-feed", "v1")]);
        _repository.Save([Feed("https://xn--bcher-kva.de/google-feed", "v2")]);

        var stored = Assert.Single(_dbContext.FeedData.AsNoTracking().ToList());
        Assert.Equal("v2", Encoding.UTF8.GetString(stored.FeedBytes));
    }

    [Fact]
    public async Task SaveAsync_ExistingLink_UpdatesFeedInsteadOfAddingRow()
    {
        await _repository.SaveAsync([Feed("https://example.com/google-feed", "v1")], CancellationToken.None);
        await _repository.SaveAsync([Feed("https://example.com/google-feed", "v2")], CancellationToken.None);

        var stored = Assert.Single(_dbContext.FeedData.AsNoTracking().ToList());
        Assert.Equal("v2", Encoding.UTF8.GetString(stored.FeedBytes));
    }

    [Fact]
    public void FindDescriptorByUri_MatchesFeedPath()
    {
        var descriptor = _repository.FindDescriptorByUri(new Uri("https://example.com/Google-Feed?x=1"));

        Assert.Equal("google", descriptor?.Name);
    }

    [Fact]
    public void CommandTimeout_IsAppliedToDbContext()
    {
        // Large feeds can exceed the default 30s SQL command timeout (issue #13).
        using var context = new FeedApplicationDbContext("Server=.;Database=ProductFeed", TimeSpan.FromMinutes(5));

        Assert.Equal(300, context.Database.GetCommandTimeout());
    }

    private static FeedEntity Feed(string link, string content) =>
        new() { Link = link, FeedBytes = Encoding.UTF8.GetBytes(content) };
}
