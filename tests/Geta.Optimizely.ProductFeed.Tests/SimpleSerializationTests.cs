using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Reference.Commerce.Site.Features.Product.Models;
using Geta.Optimizely.ProductFeed.Configuration;
using Geta.Optimizely.ProductFeed.Google;
using Geta.Optimizely.ProductFeed.Google.Models;
using Geta.Optimizely.ProductFeed.Models;
using Xunit;

namespace Geta.Optimizely.ProductFeed.Tests;

public class SimpleSerializationTests
{
    private readonly CancellationTokenSource _cts = new();

    [Fact]
    public void GenerateSimpleFeed()
    {
        var gc = new GoogleProductFeedExporter();
        gc.SetConverter(new FeedConverter());

        var enrichers = new List<IProductFeedContentEnricher> { new BrandEnricher2(), new BrandEnricher1() };
        var converters = new List<IProductFeedContentExporter> { gc };

        var sourceData = LoadSourceData()
            .Select(d =>
            {
                foreach (var enricher in enrichers)
                {
                    enricher.Enrich(d, _cts.Token);
                }

                return d;
            });

        foreach (var d in sourceData)
        {
            foreach (var converter in converters)
            {
                converter.ConvertEntry(d, _cts.Token);
            }
        }

        var result = gc.Export(_cts.Token);

        Assert.NotNull(result);
    }

    private IEnumerable<FashionProduct> LoadSourceData()
    {
        for (var i = 0; i < 10; i++)
        {
            yield return new FashionProduct { Code = i.ToString() };
        }
    }
}

public class FeedConverter : IProductFeedConverter
{
    public IFeed CreateFeed(FeedDescriptor feedDescriptor)
    {
        return new Feed();
    }

    public IFeedEntry Convert(CatalogContentBase catalogContent)
    {
        return new Entry();
    }
}

public class BrandEnricher2 : IProductFeedContentEnricher
{
    public CatalogContentBase Enrich(CatalogContentBase sourceData, CancellationToken cancellationToken)
    {
        ((FashionProduct)sourceData).Brand = "brand 1";

        return sourceData;
    }
}

public class BrandEnricher1 : IProductFeedContentEnricher
{
    public CatalogContentBase Enrich(CatalogContentBase sourceData, CancellationToken cancellationToken)
    {
        ((FashionProduct)sourceData).Brand += " %";

        return sourceData;
    }
}
