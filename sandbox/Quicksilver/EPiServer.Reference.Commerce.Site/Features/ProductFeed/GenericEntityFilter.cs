using Geta.Optimizely.ProductFeed;

namespace EPiServer.Reference.Commerce.Site.Features.ProductFeed
{
    public class GenericEntityFilter : IProductFeedFilter<MyCommerceProductRecord>
    {
        public bool ShouldInclude(MyCommerceProductRecord entity)
        {
            return false;
        }
    }
}
