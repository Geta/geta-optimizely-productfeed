// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Web;
using Geta.Optimizely.ProductFeed;
using Geta.Optimizely.ProductFeed.Google.Models;

namespace EPiServer.Reference.Commerce.Site.Features.ProductFeed
{
    public class GoogleXmlConverter : IProductFeedConverter<MyCommerceProductRecord>
    {
        private readonly IPricingService _pricingService;

        public GoogleXmlConverter(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        public object Convert(MyCommerceProductRecord entity, HostDefinition host)
        {
            var variantCode = entity.Code;
            var defaultPrice = _pricingService.GetDefaultPrice(variantCode);

            var entry = new Entry
            {
                Id = variantCode,
                Title = entity.DisplayName,
                Description = entity.Description,
                Link = host.Url + entity.Url,
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
