// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;

namespace Geta.Optimizely.ProductFeed.Repositories;

internal static class FeedLink
{
    // Feed links are stored from HostDefinition.Url, which keeps internationalized host names in Unicode
    // (https://bücher.de/feed), while incoming requests carry the IDN-encoded host (https://xn--bcher-kva.de/feed).
    // Normalizing both to the IDN host, without query or fragment, makes them comparable.
    public static string Normalize(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        var builder = new UriBuilder(uri) { Host = uri.IdnHost, Query = string.Empty, Fragment = string.Empty };

        return builder.Uri.AbsoluteUri.TrimEnd('/');
    }

    public static string Normalize(string link)
    {
        return Uri.TryCreate(link, UriKind.Absolute, out var uri) ? Normalize(uri) : link;
    }

    public static bool AreEqual(string link, string normalizedLink)
    {
        return string.Equals(Normalize(link), normalizedLink, StringComparison.OrdinalIgnoreCase);
    }
}
