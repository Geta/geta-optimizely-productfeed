using System.Collections.Generic;
using System.Linq;
using EPiServer.Web;

namespace Geta.Optimizely.ProductFeed;

public interface ISiteBuilder
{
    IEnumerable<HostDefinition> GetHosts();
}

public class DefaultSiteBuilder : ISiteBuilder
{
    private readonly ISiteDefinitionRepository _siteDefinitionRepository;

    public DefaultSiteBuilder(ISiteDefinitionRepository siteDefinitionRepository)
    {
        _siteDefinitionRepository = siteDefinitionRepository;
    }

    public IEnumerable<HostDefinition> GetHosts()
    {
        return _siteDefinitionRepository
            .List()
            .SelectMany(sd => sd.Hosts)
            .Where(h => h.Url != null);
    }
}
