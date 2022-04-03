using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Geta.Optimizely.ProductFeed.Configuration
{
    public static class IEndpointRouteBuilderExtensions
    {
        public static IEndpointRouteBuilder MapProductFeeds(this IEndpointRouteBuilder endpoints)
        {
            var descriptors = endpoints.ServiceProvider.GetServices<FeedDescriptor>();

            foreach (var descriptor in descriptors)
            {
                endpoints.MapControllerRoute(
                    descriptor.Name + "-endpoint",
                    descriptor.FileName.TrimStart('/'),
                    new
                    {
                        controller = nameof(ProductFeedController).Replace(nameof(Controller), string.Empty),
                        action = nameof(ProductFeedController.Get)
                    });
            }

            return endpoints;
        }
    }
}
