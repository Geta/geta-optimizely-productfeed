// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Geta.Optimizely.GoogleProductFeed.Repositories;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;
using Microsoft.EntityFrameworkCore;

namespace Geta.Optimizely.ProductFeed.Repositories;

public class FeedRepository(FeedApplicationDbContext applicationDbContext, IEnumerable<FeedDescriptor> descriptors) : IFeedRepository
{
    public FeedEntity GetLatestFeed(Uri siteUri)
    {
        ArgumentNullException.ThrowIfNull(siteUri);

        var link = FeedLink.Normalize(siteUri);

        // Links are stored normalized, so one query usually finds the feed. Rows saved by earlier versions keep
        // their original link until the job regenerates them, so fall back to matching links client-side.
        return applicationDbContext.FeedData
                   .AsNoTracking()
                   .Where(f => f.Link == link)
                   .OrderByDescending(f => f.CreatedUtc)
                   .FirstOrDefault()
               ?? FindFeed(FindFeedId(link));
    }

    public void Save(ICollection<FeedEntity> feedData)
    {
        if (feedData == null)
        {
            return;
        }

        foreach (var data in feedData)
        {
            var link = FeedLink.NormalizeStored(data.Link);
            Upsert(FindFeedId(link), link, data);

            applicationDbContext.SaveChanges();
            applicationDbContext.ChangeTracker.Clear();
        }
    }

    public async Task SaveAsync(ICollection<FeedEntity> feedData, CancellationToken cancellationToken)
    {
        if (feedData == null)
        {
            return;
        }

        foreach (var data in feedData)
        {
            var link = FeedLink.NormalizeStored(data.Link);
            Upsert(await FindFeedIdAsync(link, cancellationToken), link, data);

            await applicationDbContext.SaveChangesAsync(cancellationToken);
            applicationDbContext.ChangeTracker.Clear();
        }
    }

    public FeedDescriptor FindDescriptorByUri(Uri siteUri)
    {
        ArgumentNullException.ThrowIfNull(siteUri);

        // AbsolutePath is percent-encoded ("produktfl%C3%B6de"), descriptor file names are not ("produktflöde").
        var path = Uri.UnescapeDataString(siteUri.AbsolutePath).Trim('/');

        return descriptors.FirstOrDefault(d => d.FileName.Trim('/').Equals(path, StringComparison.InvariantCultureIgnoreCase));
    }

    // Only Id and Link are loaded: links are matched client-side (IDN host normalization), and loading
    // FeedBytes for every stored feed would pull the content of all feeds into memory on each lookup.
    private IQueryable<FeedLinkRow> QueryFeedLinks()
    {
        return applicationDbContext.FeedData
            .AsNoTracking()
            .OrderByDescending(f => f.CreatedUtc)
            .Select(f => new FeedLinkRow(f.Id, f.Link));
    }

    private int? FindFeedId(string normalizedLink)
    {
        return MatchFeedId(QueryFeedLinks().ToList(), normalizedLink);
    }

    private async Task<int?> FindFeedIdAsync(string normalizedLink, CancellationToken cancellationToken)
    {
        return MatchFeedId(await QueryFeedLinks().ToListAsync(cancellationToken), normalizedLink);
    }

    private static int? MatchFeedId(IEnumerable<FeedLinkRow> feeds, string normalizedLink)
    {
        return feeds.FirstOrDefault(f => FeedLink.AreEqual(f.Link, normalizedLink))?.Id;
    }

    private FeedEntity FindFeed(int? id)
    {
        return id == null ? null : applicationDbContext.FeedData.AsNoTracking().FirstOrDefault(f => f.Id == id);
    }

    private void Upsert(int? existingId, string normalizedLink, FeedEntity data)
    {
        if (existingId is int id)
        {
            // Update through a key-only entity instead of loading the existing row, whose FeedBytes can be
            // tens of megabytes. Marking the columns modified also writes a null FeedBytes, and rewrites
            // links stored by earlier versions in their normalized form.
            var existing = new FeedEntity { Id = id, CreatedUtc = DateTime.UtcNow, FeedBytes = data.FeedBytes, Link = normalizedLink };
            var entry = applicationDbContext.FeedData.Attach(existing);
            entry.Property(f => f.CreatedUtc).IsModified = true;
            entry.Property(f => f.FeedBytes).IsModified = true;
            entry.Property(f => f.Link).IsModified = true;
        }
        else
        {
            data.CreatedUtc = DateTime.UtcNow;
            data.Link = normalizedLink;
            applicationDbContext.FeedData.Add(data);
        }
    }

    private sealed record FeedLinkRow(int Id, string Link);
}
