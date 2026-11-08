// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;

namespace Geta.Optimizely.ProductFeed.Repositories;

internal static class FeedLink
{
    // Feed links are stored from HostDefinition.Url, which keeps internationalized host names in Unicode
    // (https://bücher.de/feed), while incoming requests carry the IDN-encoded host (https://xn--bcher-kva.de/feed).
    // Normalizing both to the IDN host, without fragment or trailing slash, makes them comparable.

    // A request URL also loses its query, so a request with tracking parameters (?utm_source=...) still finds the feed.
    public static string Normalize(Uri uri)
    {
        return Normalize(uri, keepQuery: false);
    }

    // A stored link keeps its query: feeds whose links differ only by query are separate feeds.
    public static string NormalizeStored(string link)
    {
        return Uri.TryCreate(link, UriKind.Absolute, out var uri) ? Normalize(uri, keepQuery: true) : link;
    }

    public static bool AreEqual(string storedLink, string normalizedLink)
    {
        return string.Equals(NormalizeStored(storedLink), normalizedLink, StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(Uri uri, bool keepQuery)
    {
        ArgumentNullException.ThrowIfNull(uri);

        var builder = new UriBuilder(uri) { Host = uri.IdnHost, Fragment = string.Empty };
        if (!keepQuery)
        {
            builder.Query = string.Empty;
        }

        return builder.Uri.AbsoluteUri.TrimEnd('/');
    }
}
