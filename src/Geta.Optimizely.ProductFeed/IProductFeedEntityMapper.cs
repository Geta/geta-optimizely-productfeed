using EPiServer.Commerce.Catalog.ContentTypes;
using Geta.Optimizely.ProductFeed.Models;

namespace Geta.Optimizely.ProductFeed
{
    public interface IProductFeedEntityMapper
    {
        Feed GenerateFeedEntity();
        Entry GenerateEntry(CatalogContentBase catalogContent);
    }
}
