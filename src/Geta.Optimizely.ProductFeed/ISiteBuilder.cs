using System.Collections.Generic;
using System.Linq;
using EPiServer.Web;

namespace Geta.Optimizely.ProductFeed;

public interface ISiteBuilder
{
    IEnumerable<HostDefinition> GetHosts();
}

public class DefaultSiteBuilder(ISiteDefinitionRepository siteDefinitionRepository) : ISiteBuilder
{
    public IEnumerable<HostDefinition> GetHosts()
    {
        return siteDefinitionRepository
            .List()
            .SelectMany(sd => sd.Hosts)
            .Where(h => h.Url != null);
    }
}
