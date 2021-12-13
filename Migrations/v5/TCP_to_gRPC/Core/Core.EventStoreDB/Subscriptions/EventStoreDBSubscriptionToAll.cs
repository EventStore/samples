using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using Core.EventStoreDB.Events;
using Core.Threading;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using Microsoft.Extensions.Logging;

namespace Core.EventStoreDB.Subscriptions;

public class EventStoreDBSubscriptionToAllOptions
{
    public string SubscriptionId { get; set; } = "default";
    public Filter Filter { get; set; } = Filter.ExcludeSystemEvents;
    public CatchUpSubscriptionFilteredSettings FilteredSettings { get; set; } =
        CatchUpSubscriptionFilteredSettings.Default;
    public UserCredentials? Credentials { get; set; }
    public bool ResolveLinkTos { get; set; }
    public bool IgnoreDeserializationErrors { get; set; } = true;
}

public class EventStoreDBSubscriptionToAll
{
    private readonly IEventBus eventBus;
    private readonly Func<IEventStoreConnection> connectToEventStore;
    private readonly ISubscriptionCheckpointRepository checkpointRepository;
    private readonly ILogger<EventStoreDBSubscriptionToAll> logger;
    private EventStoreDBSubscriptionToAllOptions subscriptionOptions = default!;
    private string SubscriptionId => subscriptionOptions.SubscriptionId;
    private readonly object resubscribeLock = new();
    private CancellationToken cancellationToken;

    public EventStoreDBSubscriptionToAll(
        Func<IEventStoreConnection> connectToEventStore,
        IEventBus eventBus,
        ISubscriptionCheckpointRepository checkpointRepository,
        ILogger<EventStoreDBSubscriptionToAll> logger
    )
    {
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        this.connectToEventStore = connectToEventStore ?? throw new ArgumentNullException(nameof(connectToEventStore));
        this.checkpointRepository =
            checkpointRepository ?? throw new ArgumentNullException(nameof(checkpointRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SubscribeToAll(EventStoreDBSubscriptionToAllOptions subscriptionOptions, CancellationToken ct)
    {
        this.subscriptionOptions = subscriptionOptions;
        cancellationToken = ct;

        logger.LogInformation("Subscription to all '{SubscriptionId}'", subscriptionOptions.SubscriptionId);

        var checkpoint = await checkpointRepository.Load(SubscriptionId, ct);

        connectToEventStore().FilteredSubscribeToAllFrom(
            checkpoint != null? new Position(checkpoint.Value, checkpoint.Value): null,
            subscriptionOptions.Filter,
            subscriptionOptions.FilteredSettings,
            HandleEvent,
            HandleStart,
            HandleDrop,
            subscriptionOptions.Credentials
        );

        logger.LogInformation("Subscription to all '{SubscriptionId}' started", SubscriptionId);
    }

    private void HandleStart(EventStoreCatchUpSubscription obj)
    {

    }

    private async Task HandleEvent(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, ResolvedEvent resolvedEvent)
    {
        try
        {
            if (IsEventWithEmptyData(resolvedEvent) || IsCheckpointEvent(resolvedEvent)) return;

            var streamEvent = resolvedEvent.ToStreamEvent();

            if (streamEvent == null)
            {
                // That can happen if we're sharing database between modules.
                // If we're subscribing to all and not filtering out events from other modules,
                // then we might get events that are from other module and we might not be able to deserialize them.
                // In that case it's safe to ignore deserialization error.
                // You may add more sophisticated logic checking if it should be ignored or not.
                logger.LogWarning("Couldn't deserialize event with id: {EventId}", resolvedEvent.Event.EventId);

                if (!subscriptionOptions.IgnoreDeserializationErrors)
                    throw new InvalidOperationException(
                        $"Unable to deserialize event {resolvedEvent.Event.EventType} with id: {resolvedEvent.Event.EventId}"
                    );

                return;
            }

            // publish event to internal event bus
            await eventBus.Publish(streamEvent, default);

            await checkpointRepository.Store(SubscriptionId, resolvedEvent.OriginalPosition!.Value.CommitPosition, default);
        }
        catch (Exception e)
        {
            logger.LogError("Error consuming message: {ExceptionMessage}{ExceptionStackTrace}", e.Message,
                e.StackTrace);
            // if you're fine with dropping some events instead of stopping subscription
            // then you can add some logic if error should be ignored
            throw;
        }
    }

    private void HandleDrop(EventStoreCatchUpSubscription eventStoreCatchUpSubscription, SubscriptionDropReason reason, Exception exception)
    {
        logger.LogError(
            exception,
            "Subscription to all '{SubscriptionId}' dropped with '{Reason}'",
            SubscriptionId,
            reason
        );

        Resubscribe();
    }

    private void Resubscribe()
    {
        // You may consider adding a max resubscribe count if you want to fail process
        // instead of retrying until database is up
        while (true)
        {
            var resubscribed = false;
            try
            {
                Monitor.Enter(resubscribeLock);

                // No synchronization context is needed to disable synchronization context.
                // That enables running asynchronous method not causing deadlocks.
                // As this is a background process then we don't need to have async context here.
                using (NoSynchronizationContextScope.Enter())
                {
                    SubscribeToAll(subscriptionOptions, cancellationToken).Wait(cancellationToken);
                }

                resubscribed = true;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception,
                    "Failed to resubscribe to all '{SubscriptionId}' dropped with '{ExceptionMessage}{ExceptionStackTrace}'",
                    SubscriptionId, exception.Message, exception.StackTrace);
            }
            finally
            {
                Monitor.Exit(resubscribeLock);
            }

            if (resubscribed)
                break;

            // Sleep between reconnections to not flood the database or not kill the CPU with infinite loop
            // Randomness added to reduce the chance of multiple subscriptions trying to reconnect at the same time
            Thread.Sleep(1000 + new Random((int)DateTime.UtcNow.Ticks).Next(1000));
        }
    }

    private bool IsEventWithEmptyData(ResolvedEvent resolvedEvent)
    {
        if (resolvedEvent.Event.Data.Length != 0) return false;

        logger.LogInformation("Event without data received");
        return true;
    }

    private bool IsCheckpointEvent(ResolvedEvent resolvedEvent)
    {
        if (resolvedEvent.Event.EventType != EventTypeMapper.ToName<CheckpointStored>()) return false;

        logger.LogInformation("Checkpoint event - ignoring");
        return true;
    }
}
