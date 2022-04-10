using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using CsvHelper;
using CsvHelper.Configuration;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using Geta.Optimizely.ProductFeed;
using Geta.Optimizely.ProductFeed.Models;

namespace EPiServer.Reference.Commerce.Site.Features.CsvFeed
{
    public class CsvExporter : AbstractFeedContentExporter
    {
        private readonly CsvWriter _writer;

        public CsvExporter()
        {
            _writer = new CsvWriter(new StreamWriter(Buffer), new CsvConfiguration(CultureInfo.InvariantCulture));
        }

        public override void BeginExport(CancellationToken cancellationToken)
        {
            _writer.WriteHeader<CsvEntry>();
            _writer.NextRecord();
        }

        public override byte[] SerializeEntry(object value, CancellationToken cancellationToken)
        {
            _writer.WriteRecord(value);
            _writer.NextRecord();

            return Array.Empty<byte>();
        }

        public override ICollection<FeedEntity> FinishExport(CancellationToken cancellationToken)
        {
            _writer.Flush();

            return base.FinishExport(cancellationToken);
        }

        public override object ConvertEntry(CatalogContentBase catalogContentBase, CancellationToken cancellationToken)
        {
            return Converter.Convert(catalogContentBase);
        }
    }

    public class CsvConverter : IProductFeedConverter
    {
        public object Convert(CatalogContentBase catalogContent)
        {
            return catalogContent is not FashionProduct ? null : new CsvEntry { Code = catalogContent.Name, Price = 1.0M };
        }
    }

    public class CsvEntry
    {
        public string Code { get; set; }
        public decimal Price { get; set; }
    }
}
