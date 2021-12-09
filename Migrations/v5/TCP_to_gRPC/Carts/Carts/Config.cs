using Carts.Carts;
using Carts.Carts.GettingCartById;
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
        services.AddElasticsearch(config,
            settings => settings.DefaultMappingFor<CartDetails>(m => m.Ignore(cd => cd.TotalPrice)));
        services.AddCarts();
    }
}
