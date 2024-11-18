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
        if (siteUri == null)
        {
            throw new ArgumentNullException(nameof(siteUri));
        }

        // we need to do client-side eval because string.Equals with comparison is not supported
        var feedContent = applicationDbContext
            .FeedData
            .ToList();

        return feedContent.FirstOrDefault(f => f.Link.Equals(GetAbsoluteUrlWithoutQuery(siteUri).AbsoluteUri.TrimEnd('/'),
                                                             StringComparison.InvariantCultureIgnoreCase));
    }

    public void Save(ICollection<FeedEntity> feedData)
    {
        if (feedData == null)
        {
            return;
        }

        var feeds = applicationDbContext.FeedData.ToList();

        foreach (var data in feedData)
        {
            PrepareFeedData(feeds, data);
            applicationDbContext.SaveChanges();
        }
    }

    public async Task SaveAsync(ICollection<FeedEntity> feedData, CancellationToken cancellationToken)
    {
        if (feedData == null)
        {
            return;
        }

        var feeds = await applicationDbContext.FeedData.ToListAsync(cancellationToken);
        foreach (var data in feedData)
        {
            PrepareFeedData(feeds, data);
            await applicationDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public FeedDescriptor FindDescriptorByUri(Uri siteUri)
    {
        var path = GetAbsoluteUrlWithoutQuery(siteUri).AbsolutePath.Trim('/');

        return descriptors.FirstOrDefault(d => d.FileName.Trim('/').Equals(path, StringComparison.InvariantCultureIgnoreCase));
    }

    private void PrepareFeedData(List<FeedEntity> feeds, FeedEntity data)
    {
        var found = feeds.FirstOrDefault(f => f.Link.Equals(data.Link, StringComparison.InvariantCultureIgnoreCase));

        if (found != null)
        {
            found.CreatedUtc = DateTime.UtcNow;
            found.FeedBytes = data.FeedBytes;
        }
        else
        {
            data.CreatedUtc = DateTime.UtcNow;
            applicationDbContext.FeedData.Add(data);
        }
    }

    private Uri GetAbsoluteUrlWithoutQuery(Uri siteUri)
    {
        return new UriBuilder(siteUri) { Query = string.Empty }.Uri;
    }
}
