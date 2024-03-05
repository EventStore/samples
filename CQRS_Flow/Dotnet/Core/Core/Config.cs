using Core.Commands;
using Core.Events;
using Core.Ids;
using Core.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core;

public static class Config
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services
            .AddScoped<ICommandBus, CommandBus>()
            .AddScoped<IQueryBus, QueryBus>();

        services.TryAddSingleton<IEventBus, EventBus>();
        services.TryAddScoped<IIdGenerator, NulloIdGenerator>();

        return services;
    }
}
