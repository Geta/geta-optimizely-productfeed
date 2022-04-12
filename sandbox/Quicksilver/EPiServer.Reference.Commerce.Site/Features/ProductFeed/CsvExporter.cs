using Geta.Optimizely.ProductFeed;
using Geta.Optimizely.ProductFeed.Csv;

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
}
