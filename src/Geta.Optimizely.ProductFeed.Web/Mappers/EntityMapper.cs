using System.Linq;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using Foundation.Features.CatalogContent.Product;
using Foundation.Features.CatalogContent.Variation;
using Foundation.Infrastructure.Commerce.Extensions;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Web.Models;

namespace Geta.Optimizely.ProductFeed.Web.Mappers
{
    public class EntityMapper : IEntityMapper<MyCommerceProductRecord>
    {
        private readonly IContentLoader _contentLoader;

        public EntityMapper(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        public MyCommerceProductRecord Map(CatalogContentBase catalogContent)
        {
            if (catalogContent is GenericVariant content)
            {
                var variationContent = content;
                var product = _contentLoader.Get<CatalogContentBase>(variationContent.GetParentProducts().FirstOrDefault()) as GenericProduct;

                if (product != null)
                {
                    var productRecord = new MyCommerceProductRecord
                    {
                        Code = product.Code,
                        DisplayName = variationContent.DisplayName,
                        Description = product.Description.ToHtmlString(),
                        Url = variationContent.GetUrl(),
                        Brand = product.Brand
                    };

                    var image = variationContent.GetDefaultAsset<IContentImage>();
                    if (!string.IsNullOrEmpty(image))
                    {
                        productRecord.ImageLink = image;
                    }

                    return productRecord;
                }
            }

            return null;
        }
    }
}
