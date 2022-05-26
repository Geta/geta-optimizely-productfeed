using EPiServer.Web;
using Geta.Optimizely.ProductFeed;

namespace EPiServer.Reference.Commerce.Site.Features.ProductFeed
{

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
}
