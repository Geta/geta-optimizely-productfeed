// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace Geta.Optimizely.ProductFeed.Tests;

public class JobStatusLoggerTests
{
    private readonly List<string> _statuses = [];

    [Fact]
    public void Progress_ReportsEveryFivePercentAndCompletion()
    {
        var logger = new JobStatusLogger(_statuses.Add);

        for (var i = 1; i <= 45; i++)
        {
            logger.Progress("Working", i, 45);
        }

        // step = 45 / 20 = 2 -> every second item, plus the final one
        Assert.Equal(23, _statuses.Count);
        Assert.Equal("Working: 2 of 45 (4%)", _statuses[0]);
        Assert.Equal("Working: 45 of 45 (100%)", _statuses[^1]);
    }

    [Fact]
    public void Progress_DoesNotWriteToJobLog()
    {
        var logger = new JobStatusLogger(_statuses.Add);

        logger.Progress("Working", 1, 1);

        Assert.Equal(string.Empty, logger.ToString());
    }

    [Fact]
    public void Progress_IgnoresEmptyWork()
    {
        new JobStatusLogger(_statuses.Add).Progress("Working", 0, 0);

        Assert.Empty(_statuses);
    }

    [Fact]
    public void LogWithStatus_UsesInvariant24HourTimestamp()
    {
        var logger = new JobStatusLogger(_statuses.Add, new FixedTimeProvider(new DateTimeOffset(2026, 11, 7, 14, 30, 5, TimeSpan.Zero)));
        var culture = CultureInfo.CurrentCulture;

        try
        {
            // fi-FI formats times with '.' as the separator
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fi-FI");
            logger.LogWithStatus("done");
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }

        Assert.Equal("2026-11-07 14:30:05 - done", _statuses[0]);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
