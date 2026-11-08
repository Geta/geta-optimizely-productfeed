// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Text;

namespace Geta.Optimizely.ProductFeed;

public class JobStatusLogger(Action<string> onStatusChanged)
{
    private readonly StringBuilder _stringBuilder = new();

    public void Log(string message)
    {
        _stringBuilder.AppendLine(message);
    }

    public void LogWithStatus(string message)
    {
        message = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}";
        Status(message);
        Log(message);
    }

    public void Status(string message)
    {
        onStatusChanged?.Invoke(message);
    }

    // Updates the job status (not the job log) roughly every 5% of the work, and when the work is done.
    public void Progress(string activity, int processed, int total)
    {
        if (total <= 0)
        {
            return;
        }

        var step = Math.Max(1, total / 20);
        if (processed % step != 0 && processed != total)
        {
            return;
        }

        Status($"{activity}: {processed} of {total} ({processed * 100L / total}%)");
    }

    public string ToString(string separator = "<br />")
    {
        return _stringBuilder?.ToString().Replace(Environment.NewLine, separator);
    }
}
