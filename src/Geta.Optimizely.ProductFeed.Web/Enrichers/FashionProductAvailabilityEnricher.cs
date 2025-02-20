using System.Threading;
using Geta.Optimizely.ProductFeed.Web.Models;

namespace Geta.Optimizely.ProductFeed.Web.Enrichers
{
    public class FashionProductAvailabilityEnricher : IProductFeedContentEnricher<MyCommerceProductRecord>
    {
        public MyCommerceProductRecord Enrich(MyCommerceProductRecord sourceData, CancellationToken cancellationToken)
        {
            sourceData.IsAvailable = true;
            return sourceData;
        }
    }
}
