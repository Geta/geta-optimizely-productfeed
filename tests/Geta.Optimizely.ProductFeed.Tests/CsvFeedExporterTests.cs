// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading;
using EPiServer.Web;
using Geta.Optimizely.ProductFeed.Csv;
using Xunit;

namespace Geta.Optimizely.ProductFeed.Tests;

public class CsvFeedExporterTests
{
    private static readonly HostDefinition Host = new() { Name = "example.com" };

    [Fact]
    public void SingleRecord_WritesOneRow()
    {
        var csv = Export(new CsvRow { Code = "A", Name = "Shirt" });

        Assert.Equal(["Code,Name", "A,Shirt"], csv);
    }

    [Fact]
    public void CollectionOfRecords_WritesOneRowPerItem()
    {
        var csv = Export(new[]
        {
            new CsvRow { Code = "A", Name = "Image 1" },
            new CsvRow { Code = "A", Name = "Image 2" }
        });

        Assert.Equal(["Code,Name", "A,Image 1", "A,Image 2"], csv);
    }

    [Fact]
    public void DynamicRecord_WritesOneRow()
    {
        IDictionary<string, object?> record = new ExpandoObject();
        record["Code"] = "A";
        record["Name"] = "Shirt";

        var csv = Export(record);

        Assert.Equal(["Code,Name", "A,Shirt"], csv);
    }

    [Fact]
    public void DictionaryRecord_WritesOneRow()
    {
        var csv = Export(new Dictionary<string, object> { ["Code"] = "A", ["Name"] = "Shirt" });

        Assert.Equal(["Code,Name", "A,Shirt"], csv);
    }

    [Fact]
    public void CollectionWithNullItems_SkipsNulls()
    {
        var csv = Export(new[] { new CsvRow { Code = "A", Name = "x" }, null });

        Assert.Equal(["Code,Name", "A,x"], csv);
    }

    [Fact]
    public void CollectionOfRecords_HonorsCancellation()
    {
        var exporter = CreateExporter(_ => new[] { new CsvRow(), new CsvRow() });
        using var cts = new CancellationTokenSource();
        exporter.BeginExport(Host, cts.Token);
        cts.Cancel();

        Assert.Throws<OperationCanceledException>(() => exporter.BuildEntry(new object(), Host, cts.Token));
    }

    private static string[] Export(object converted)
    {
        var exporter = CreateExporter(_ => converted);
        exporter.BeginExport(Host, CancellationToken.None);
        exporter.BuildEntry(new object(), Host, CancellationToken.None);
        var feed = exporter.FinishExport(Host, CancellationToken.None).Single();

        return Encoding.UTF8.GetString(feed.FeedBytes)
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
    }

    private static CsvFeedExporter<object> CreateExporter(Func<object, object> convert)
    {
        var descriptor = new CsvFeedDescriptor<object> { CsvEntityType = typeof(CsvRow), FileName = "/csv-feed" };
        var exporter = new CsvFeedExporter<object>(descriptor);
        exporter.SetDescriptor(descriptor);
        exporter.SetConverter(new DelegateConverter(convert));

        return exporter;
    }

    public class CsvRow
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    private class DelegateConverter(Func<object, object> convert) : IProductFeedConverter<object>
    {
        public object Convert(object entity, HostDefinition host) => convert(entity);
    }
}
