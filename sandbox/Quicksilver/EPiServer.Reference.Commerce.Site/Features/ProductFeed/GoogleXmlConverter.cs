// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using EPiServer.Reference.Commerce.Site.Features.Shared.Extensions;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Web;
using Geta.Optimizely.ProductFeed;
using Geta.Optimizely.ProductFeed.Google.Models;

namespace EPiServer.Reference.Commerce.Site.Features.ProductFeed
{
    public class GoogleXmlConverter : IProductFeedConverter<MyCommerceProductRecord>
    {
        private readonly IContentLoader _contentLoader;
        private readonly IPricingService _pricingService;
        private readonly string _siteUrl;

        public GoogleXmlConverter(
            IContentLoader contentLoader,
            IPricingService pricingService,
            ISiteDefinitionRepository siteDefinitionRepository)
        {
            _contentLoader = contentLoader;
            _pricingService = pricingService;
            _siteUrl = siteDefinitionRepository.List().FirstOrDefault()?.SiteUrl.ToString();
        }

        public object Convert(MyCommerceProductRecord entity)
        {
            var variantCode = entity.Code;
            var defaultPrice = _pricingService.GetDefaultPrice(variantCode);

            var entry = new Entry
            {
                Id = variantCode,
                Title = entity.DisplayName,
                Description = entity.Description,
                Link = entity.Url,
                Condition = "new",
                Availability = "in stock",
                Brand = entity.Brand,
                MPN = string.Empty,
                GTIN = "725272730706",
                GoogleProductCategory = string.Empty,
                ImageLink = entity.ImageLink,
                Shipping = new List<Shipping>
                {
                    new()
                    {
                        Price = "1 USD",
                        Country = "US",
                        Service = "Standard"
                    }
                }
            };

            if (defaultPrice != null)
            {
                var discountPrice = _pricingService.GetDiscountPrice(variantCode);

                entry.Price = defaultPrice.UnitPrice.FormatPrice();
                entry.SalePrice = discountPrice != null ? discountPrice.UnitPrice.FormatPrice() : string.Empty;
                entry.SalePriceEffectiveDate = $"{DateTime.UtcNow:yyyy-MM-ddThh:mm:ss}/{DateTime.UtcNow.AddDays(7):yyyy-MM-ddThh:mm:ss}";
            }

            return entry;
        }
    }
}
