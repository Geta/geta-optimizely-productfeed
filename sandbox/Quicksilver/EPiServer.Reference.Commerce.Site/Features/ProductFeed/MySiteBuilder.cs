using System.Collections.Generic;
using System.Linq;
using EPiServer.Web;
using Geta.Optimizely.ProductFeed;

namespace EPiServer.Reference.Commerce.Site.Features.ProductFeed
{
    public class MySiteBuilder : ISiteBuilder
    {
        private readonly ISiteDefinitionRepository _siteDefinitionRepository;

        public MySiteBuilder(ISiteDefinitionRepository siteDefinitionRepository)
        {
            _siteDefinitionRepository = siteDefinitionRepository;
        }

        public IEnumerable<HostDefinition> GetHosts()
        {
            return _siteDefinitionRepository
                .List()
                .First()
                .Hosts
                .Where(h => h.Url != null);
        }
    }
}
