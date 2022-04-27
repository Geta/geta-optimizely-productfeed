using Geta.Optimizely.ProductFeed;

namespace EPiServer.Reference.Commerce.Site.Features.ProductFeed
{
    public class GoogleXmlFilter : IProductFeedFilter<MyCommerceProductRecord>
    {
        public bool ShouldInclude(MyCommerceProductRecord entity)
        {
            return true;
        }
    }
}
