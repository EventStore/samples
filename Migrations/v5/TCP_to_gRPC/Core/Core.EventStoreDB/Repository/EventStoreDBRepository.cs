using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Events;
using Core.EventStoreDB.Events;
using Core.EventStoreDB.Serialization;
using Core.Repositories;
using EventStore.ClientAPI;

namespace Core.EventStoreDB.Repository;

public class EventStoreDBRepository<T>: IRepository<T> where T : class, IAggregate
{
    private readonly IEventStoreConnection eventStore;

    public EventStoreDBRepository(
        Func<IEventStoreConnection> connectToEventStore,
        IEventBus eventBus
    )
    {
        eventStore = (connectToEventStore ?? throw new ArgumentNullException(nameof(connectToEventStore)))();
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken)
    {
        return eventStore.AggregateStream<T>(id);
    }

    public Task Add(T aggregate, CancellationToken cancellationToken)
    {
        return Store(aggregate, cancellationToken);
    }

    public Task Update(T aggregate, CancellationToken cancellationToken)
    {
        return Store(aggregate, cancellationToken);
    }

    public Task Delete(T aggregate, CancellationToken cancellationToken)
    {
        return Store(aggregate, cancellationToken);
    }

    private async Task Store(T aggregate, CancellationToken cancellationToken)
    {
        var events = aggregate.DequeueUncommittedEvents();

        var eventsToStore = events
            .Select(EventStoreDBSerializer.ToJsonEventData).ToArray();

        await eventStore.AppendToStreamAsync(
            StreamNameMapper.ToStreamId<T>(aggregate.Id),
            // TODO: Add proper optimistic concurrency handling
            ExpectedVersion.Any,
            eventsToStore
        );
    }
}
