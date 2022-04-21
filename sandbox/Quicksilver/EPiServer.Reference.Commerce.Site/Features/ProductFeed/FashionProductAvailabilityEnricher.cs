using System.Threading;
using Geta.Optimizely.ProductFeed;

namespace EPiServer.Reference.Commerce.Site.Features.ProductFeed
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
