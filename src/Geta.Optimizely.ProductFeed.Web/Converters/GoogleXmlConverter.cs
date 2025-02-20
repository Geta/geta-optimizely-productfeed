using System;
using System.Collections.Generic;
using EPiServer.Web;
using Foundation.Features.CatalogContent.Services;
using Geta.Optimizely.ProductFeed.Google.Models;
using Geta.Optimizely.ProductFeed.Web.Extensions;
using Geta.Optimizely.ProductFeed.Web.Models;

namespace Geta.Optimizely.ProductFeed.Web.Converters;

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
        var defaultPrice = _pricingService.GetCurrentPrice(variantCode);

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
            Shipping = new List<Shipping> { new() { Price = "1 USD", Country = "US", Service = "Standard" } }
        };

        if (defaultPrice != null)
        {
            entry.Price = defaultPrice.Value.FormatPrice();
            entry.SalePriceEffectiveDate =
                $"{DateTime.UtcNow:yyyy-MM-ddThh:mm:ss}/{DateTime.UtcNow.AddDays(7):yyyy-MM-ddThh:mm:ss}";
        }

        return entry;
    }
}
