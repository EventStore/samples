using Carts.Carts;
using Core.ElasticSearch;
using Core.EventStoreDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Carts;

public static class Config
{
    public static void AddCartsModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddEventStoreDB(config);
        // Document Part used for projections
        services.AddElasticsearch(config);
        services.AddCarts();
    }
}