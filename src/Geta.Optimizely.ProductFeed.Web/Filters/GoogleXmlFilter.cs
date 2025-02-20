using Geta.Optimizely.ProductFeed.Web.Models;

namespace Geta.Optimizely.ProductFeed.Web.Filters;

public class GoogleXmlFilter : IProductFeedFilter<MyCommerceProductRecord>
{
    public bool ShouldInclude(MyCommerceProductRecord entity)
    {
        return true;
    }
}
