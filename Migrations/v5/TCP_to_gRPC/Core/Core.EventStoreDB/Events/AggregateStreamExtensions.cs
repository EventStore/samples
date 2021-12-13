using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Events;
using Core.EventStoreDB.Serialization;
using Core.Exceptions;
using Core.Projections;
using EventStore.ClientAPI;

namespace Core.EventStoreDB.Events;

public static class AggregateStreamExtensions
{
    public static async Task<T?> AggregateStream<T>(
        this IEventStoreConnection eventStore,
        Guid id,
        long? fromVersion = null
    ) where T : class, IProjection
    {
        var readResult = await eventStore.ReadStreamEventsForwardAsync(
            StreamNameMapper.ToStreamId<T>(id),
            fromVersion ?? StreamPosition.Start,
            ClientApiConstants.MaxReadSize,
            false
        );

        if(readResult.Status != SliceReadStatus.Success)
            throw AggregateNotFoundException.For<T>(id);

        var entity = (T)Activator.CreateInstance(typeof(T), true)!;

        return readResult.Events
            .Select(@event => @event.Deserialize()!)
            .Aggregate(entity, (state, @event) =>
            {
                state.When(@event!);

                return state;
            });
    }
}
