using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using Geta.Optimizely.ProductFeed;
using Geta.Optimizely.ProductFeed.Csv;

namespace EPiServer.Reference.Commerce.Site.Features.CsvFeed
{

    public class CsvConverter : IProductFeedConverter<CatalogContentBase>
    {
        public object Convert(CatalogContentBase catalogContent)
        {
            return catalogContent is not FashionProduct ? null : new CsvEntry { Code = catalogContent.Name, Price = 1.0M };
        }
    }
}
