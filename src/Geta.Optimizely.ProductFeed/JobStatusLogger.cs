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
        message = $"{DateTime.UtcNow:yyyy-MM-dd hh:mm:ss} - {message}";
        Status(message);
        Log(message);
    }

    public void Status(string message)
    {
        onStatusChanged?.Invoke(message);
    }

    public string ToString(string separator = "<br />")
    {
        return _stringBuilder?.ToString().Replace(Environment.NewLine, separator);
    }
}
