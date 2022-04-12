using Geta.Optimizely.ProductFeed;

namespace EPiServer.Reference.Commerce.Site.Features.ProductFeed
{

    public class CsvConverter : IProductFeedConverter<MyCommerceProductRecord>
    {
        public object Convert(MyCommerceProductRecord catalogContent)
        {
            return new CsvEntry
            {
                Code = catalogContent.Code,
                Name = catalogContent.DisplayName,
                Price = 1.0M
            };
        }
    }

    public class CsvEntry
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Price { get; set; }
    }
}
