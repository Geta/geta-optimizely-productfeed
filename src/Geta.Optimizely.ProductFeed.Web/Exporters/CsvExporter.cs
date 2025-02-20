using EPiServer.Web;
using Geta.Optimizely.ProductFeed.Web.Models;

namespace Geta.Optimizely.ProductFeed.Web.Exporters;

public class CsvConverter : IProductFeedConverter<MyCommerceProductRecord>
{
    public object Convert(MyCommerceProductRecord entity, HostDefinition host)
    {
        return new CsvEntry
        {
            Code = entity.Code,
            Name = entity.DisplayName,
            IsAvailable = entity.IsAvailable,
            Price = 1.0M
        };
    }
}

public class CsvEntry
{
    public string Name { get; set; }
    public string Code { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}
