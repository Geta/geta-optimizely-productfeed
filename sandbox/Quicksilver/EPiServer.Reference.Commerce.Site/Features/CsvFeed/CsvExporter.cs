using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Web;
using Geta.Optimizely.ProductFeed;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Models;

namespace EPiServer.Reference.Commerce.Site.Features.CsvFeed
{
    public class CsvExporter : AbstractFeedContentExporter
    {
        private readonly MemoryStream _ms;
        private readonly string _siteUrl;

        public CsvExporter(ISiteDefinitionRepository siteDefinitionRepository)
        {
            _siteUrl = siteDefinitionRepository.List().FirstOrDefault()?.SiteUrl.ToString();
            _ms = new MemoryStream();
        }

        public override ICollection<FeedEntity> Export(CancellationToken cancellationToken)
        {
            return new[]
            {
                new FeedEntity
                {
                    CreatedUtc = DateTime.UtcNow,
                    Link = _siteUrl.TrimEnd('/') + '/' + "csv-feed",
                    FeedBytes = _ms.ToArray()
                }
            };
        }

        public override void ConvertEntry(CatalogContentBase catalogContentBase, CancellationToken cancellationToken)
        {
            if (Converter.Convert(catalogContentBase) is CsvEntry e)
            {
                _ms.Write(Encoding.UTF8.GetBytes($"{e.Code}\t{e.Price}"));
            }
        }
    }

    public class CsvConverter : IProductFeedConverter
    {
        public IFeed CreateFeed(FeedDescriptor feedDescriptor) { throw new NotImplementedException(); }

        public IFeedEntry Convert(CatalogContentBase catalogContent)
        {
            return new CsvEntry { Code = catalogContent.Name, Price = 1.0M };
        }
    }

    public class CsvEntry : IFeedEntry
    {
        public string Code { get; set; }
        public decimal Price { get; set; }
    }
}
