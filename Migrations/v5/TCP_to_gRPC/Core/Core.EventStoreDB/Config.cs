using System;
using System.Threading.Tasks;
using Core.BackgroundWorkers;
using Core.EventStoreDB.Connection;
using Core.EventStoreDB.Subscriptions;
using EventStore.Client;
using EventStore.ClientAPI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.EventStoreDB;

public class EventStoreDBConfig
{
    public string ConnectionString { get; set; } = default!;

    public string TcpConnectionString { get; set; } = default!;
}

public record EventStoreDBOptions(
    bool UseInternalCheckpointing = true
);

public static class EventStoreDBConfigExtensions
{
    private const string DefaultConfigKey = "EventStore";

    public static IServiceCollection AddEventStoreDB(this IServiceCollection services, IConfiguration config, EventStoreDBOptions? options = null)
    {
        var eventStoreDBConfig = config.GetSection(DefaultConfigKey).Get<EventStoreDBConfig>();

        services
            .AddSingleton(
                new EventStoreClient(EventStoreClientSettings.Create(eventStoreDBConfig.ConnectionString)))
            .AddTransient<EventStoreDBSubscriptionToAll, EventStoreDBSubscriptionToAll>();

        services.AddSingleton<EventStoreDBConnectionProvider>();
        services
            .AddTransient(sp => sp.GetRequiredService<EventStoreDBConnectionProvider>().Connect(eventStoreDBConfig.TcpConnectionString));

        // services
        //     .AddSingleton(
        //         new Lazy<Task<IEventStoreConnection>>(async () =>
        //         {
        //             var connection = EventStoreConnection.Create(new Uri(eventStoreDBConfig.TcpConnectionString));
        //             await connection.ConnectAsync();
        //
        //             return connection;
        //         }));

        if (options?.UseInternalCheckpointing != false)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDBSubscriptionCheckpointRepository>();
        }

        return services;
    }

    public static IServiceCollection AddEventStoreDBSubscriptionToAll(this IServiceCollection services,
        EventStoreDBSubscriptionToAllOptions? subscriptionOptions = null,
        bool checkpointToEventStoreDB = true)
    {
        if (checkpointToEventStoreDB)
        {
            services
                .AddTransient<ISubscriptionCheckpointRepository, EventStoreDBSubscriptionCheckpointRepository>();
        }

        return services.AddHostedService(serviceProvider =>
            {
                var logger =
                    serviceProvider.GetRequiredService<ILogger<BackgroundWorker>>();

                var eventStoreDBSubscriptionToAll =
                    serviceProvider.GetRequiredService<EventStoreDBSubscriptionToAll>();

                return new BackgroundWorker(
                    logger,
                    ct =>
                        eventStoreDBSubscriptionToAll.SubscribeToAll(
                            subscriptionOptions ?? new EventStoreDBSubscriptionToAllOptions(),
                            ct
                        )
                );
            }
        );
    }
}
