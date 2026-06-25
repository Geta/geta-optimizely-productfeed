// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Web;
using Geta.Optimizely.ProductFeed.Google;
using Geta.Optimizely.ProductFeed.Google.Models;
using Xunit;

namespace Geta.Optimizely.ProductFeed.Tests;

public class SimpleSerializationTests
{
    private readonly CancellationTokenSource _cts = new();

    [Fact]
    public void GenerateSimpleFeed()
    {
        var gc = new GoogleFeedExporter<CatalogContentBase>();
        gc.SetConverter(new FeedConverter());
        gc.SetSiteUrlBuilder(new SiteUrlBuilderForUnitTests());
        gc.SetDescriptor(new GoogleFeedDescriptor<CatalogContentBase>());

        var enrichers = new List<IProductFeedContentEnricher<CatalogContentBase>> { new BrandEnricher2(), new BrandEnricher1() };
        var converters = new List<AbstractFeedContentExporter<CatalogContentBase>> { gc };

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
                converter.ConvertEntry(d, null, _cts.Token);
            }
        }

        var result = gc.FinishExport(null, _cts.Token);

        Assert.NotNull(result);
    }

    private static IEnumerable<TestProduct> LoadSourceData()
    {
        for (var i = 0; i < 10; i++)
        {
            yield return new TestProduct { Code = i.ToString() };
        }
    }
}

// Local test product so the unit tests don't depend on the Foundation submodule
// (keeps CI restore limited to the package's own dependencies).
public class TestProduct : ProductContent
{
    public virtual string Brand { get; set; } = string.Empty;
}

public class SiteUrlBuilderForUnitTests : ISiteUrlBuilder
{
    public string BuildUrl() => string.Empty;
}

public class FeedConverter : IProductFeedConverter<CatalogContentBase>
{
    public object Convert(CatalogContentBase entity, HostDefinition host)
    {
        return new Entry();
    }
}

public class BrandEnricher2 : IProductFeedContentEnricher<CatalogContentBase>
{
    public CatalogContentBase Enrich(CatalogContentBase sourceData, CancellationToken cancellationToken)
    {
        var fashionProduct = (TestProduct)sourceData;
        fashionProduct.Brand = $"brand {fashionProduct.Code}";

        return sourceData;
    }
}

public class BrandEnricher1 : IProductFeedContentEnricher<CatalogContentBase>
{
    public CatalogContentBase Enrich(CatalogContentBase sourceData, CancellationToken cancellationToken)
    {
        ((TestProduct)sourceData).Brand += " %";

        return sourceData;
    }
}
