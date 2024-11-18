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

public class FeedRepository : IFeedRepository
{
    private readonly FeedApplicationDbContext _applicationDbContext;
    private readonly IEnumerable<FeedDescriptor> _descriptors;

    public FeedRepository(FeedApplicationDbContext applicationDbContext, IEnumerable<FeedDescriptor> descriptors)
    {
        _applicationDbContext = applicationDbContext;
        _descriptors = descriptors;
    }

    public FeedEntity GetLatestFeed(Uri siteUri)
    {
        if (siteUri == null)
        {
            throw new ArgumentNullException(nameof(siteUri));
        }

        // we need to do client-side eval because string.Equals with comparison is not supported
        var feedContent = _applicationDbContext
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

        var feeds = _applicationDbContext.FeedData.ToList();

        foreach (var data in feedData)
        {
            PrepareFeedData(feeds, data);
            _applicationDbContext.SaveChanges();
        }
    }

    public async Task SaveAsync(ICollection<FeedEntity> feedData, CancellationToken cancellationToken)
    {
        if (feedData == null)
        {
            return;
        }

        var feeds = await AsyncEnumerable.ToListAsync(_applicationDbContext.FeedData, cancellationToken);
        foreach (var data in feedData)
        {
            PrepareFeedData(feeds, data);
            await _applicationDbContext.SaveChangesAsync(cancellationToken);
        }
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
            _applicationDbContext.FeedData.Add(data);
        }
    }

    public FeedDescriptor FindDescriptorByUri(Uri siteUri)
    {
        var path = GetAbsoluteUrlWithoutQuery(siteUri).AbsolutePath.Trim('/');

        return _descriptors.FirstOrDefault(d => d.FileName.Trim('/').Equals(path, StringComparison.InvariantCultureIgnoreCase));
    }

    private Uri GetAbsoluteUrlWithoutQuery(Uri siteUri)
    {
        return new UriBuilder(siteUri) { Query = string.Empty }.Uri;
    }
}
