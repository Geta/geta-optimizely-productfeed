using System.Collections.Generic;
using EPiServer.Commerce.Catalog.ContentTypes;

namespace Geta.Optimizely.ProductFeed
{
    public interface IProductFeedContentLoader
    {
        ICollection<CatalogContentBase> LoadSourceData();
    }
}
