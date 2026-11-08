// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Threading;
using CsvHelper;
using EPiServer.Web;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed.Csv;

public class CsvFeedExporter<TEntity>(CsvFeedDescriptor<TEntity> descriptor) : AbstractFeedContentExporter<TEntity>
{
    private CsvWriter _writer;

    public override void BeginExport(HostDefinition host, CancellationToken cancellationToken)
    {
        base.BeginExport(host, cancellationToken);
        _writer = new CsvWriter(new StreamWriter(_buffer), CultureInfo.InvariantCulture);
        _writer.WriteHeader(descriptor.CsvEntityType);
        _writer.NextRecord();
    }

    public override object ConvertEntry(TEntity entity, HostDefinition host, CancellationToken cancellationToken)
    {
        return Converter.Convert(entity, host);
    }

    public override byte[] SerializeEntry(object value, CancellationToken cancellationToken)
    {
        if (IsRecordCollection(value, out var records))
        {
            foreach (var record in records)
            {
                cancellationToken.ThrowIfCancellationRequested();
                WriteRecord(record);
            }
        }
        else
        {
            WriteRecord(value);
        }

        return [];
    }

    // A converter may return a collection (e.g. IEnumerable<CsvEntry>) to write several rows for one entity.
    // A value of the declared CsvEntityType is one row even if it is enumerable, and so are dynamic objects
    // (ExpandoObject) and dictionaries, which CsvHelper writes as a single record.
    private bool IsRecordCollection(object value, out IEnumerable records)
    {
        records = value as IEnumerable;

        return records is not null and not string and not IDynamicMetaObjectProvider and not IDictionary<string, object>
               && !descriptor.CsvEntityType.IsInstanceOfType(value);
    }

    private void WriteRecord(object record)
    {
        if (record == null)
        {
            return;
        }

        _writer.WriteRecord(record);
        _writer.NextRecord();
    }

    public override ICollection<FeedEntity> FinishExport(HostDefinition host, CancellationToken cancellationToken)
    {
        _writer.Flush();

        return base.FinishExport(host, cancellationToken);
    }
}
