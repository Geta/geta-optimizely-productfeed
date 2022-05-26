using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using Geta.Optimizely.ProductFeed.Configuration;

namespace EPiServer.Reference.Commerce.Site.Features.ProductFeed
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
            if (catalogContent is FashionVariant content)
            {
                var variationContent = content;
                var product = _contentLoader.Get<CatalogContentBase>(variationContent.GetParentProducts().FirstOrDefault()) as FashionProduct;

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
