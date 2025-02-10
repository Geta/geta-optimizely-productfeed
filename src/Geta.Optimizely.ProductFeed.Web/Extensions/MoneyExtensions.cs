using System;
using Mediachase.Commerce;

namespace Geta.Optimizely.ProductFeed.Web.Extensions;

public static class MoneyExtensions
{
    public static string FormatPrice(this Money target)
    {
        var roundedPrice = Math.Round(target.Amount, 2).ToString("#.##");
        return $"{roundedPrice} {target.Currency.CurrencyCode}";
    }
}
