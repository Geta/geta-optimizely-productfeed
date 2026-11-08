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

        var id = MatchFeedId(QueryFeedLinks().ToList(), FeedLink.Normalize(siteUri));

        return id == null ? null : applicationDbContext.FeedData.AsNoTracking().FirstOrDefault(f => f.Id == id);
    }

    public void Save(ICollection<FeedEntity> feedData)
    {
        if (feedData == null)
        {
            return;
        }

        foreach (var data in feedData)
        {
            Upsert(MatchFeedId(QueryFeedLinks().ToList(), FeedLink.NormalizeStored(data.Link)), data);

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
            Upsert(MatchFeedId(await QueryFeedLinks().ToListAsync(cancellationToken), FeedLink.NormalizeStored(data.Link)), data);

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

    private static int? MatchFeedId(IEnumerable<FeedLinkRow> feeds, string normalizedLink)
    {
        return feeds.FirstOrDefault(f => FeedLink.AreEqual(f.Link, normalizedLink))?.Id;
    }

    private void Upsert(int? existingId, FeedEntity data)
    {
        if (existingId is int id)
        {
            // Update through a key-only entity instead of loading the existing row, whose FeedBytes can be
            // tens of megabytes. Marking both columns modified also writes a null FeedBytes.
            var existing = new FeedEntity { Id = id, CreatedUtc = DateTime.UtcNow, FeedBytes = data.FeedBytes };
            var entry = applicationDbContext.FeedData.Attach(existing);
            entry.Property(f => f.CreatedUtc).IsModified = true;
            entry.Property(f => f.FeedBytes).IsModified = true;
        }
        else
        {
            data.CreatedUtc = DateTime.UtcNow;
            applicationDbContext.FeedData.Add(data);
        }
    }

    private sealed record FeedLinkRow(int Id, string Link);
}
