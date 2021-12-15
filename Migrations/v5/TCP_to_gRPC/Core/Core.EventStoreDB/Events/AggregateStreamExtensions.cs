using System;
using System.Collections.Generic;
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
        var events = await eventStore.ReadStreamEvents<T>(id, fromVersion);

        var entity = (T)Activator.CreateInstance(typeof(T), true)!;

        return events
            .Select(@event => @event.Deserialize()!)
            .Aggregate(entity, (state, @event) =>
            {
                state.When(@event!);

                return state;
            });
    }

    public static async Task<IReadOnlyList<ResolvedEvent>> ReadStreamEvents<T>(this IEventStoreConnection eventStore, Guid id, long? fromVersion)
    {
        var sliceStart = fromVersion ?? StreamPosition.Start;
        var events = new List<ResolvedEvent>();
        StreamEventsSlice slice;

        do
        {
            slice = await eventStore.ReadStreamEventsForwardAsync(
                StreamNameMapper.ToStreamId<T>(id),
                sliceStart,
                200,
                false
            );

            if (slice.Status != SliceReadStatus.Success)
                throw AggregateNotFoundException.For<T>(id);

            events.AddRange(slice.Events);
            sliceStart = slice.NextEventNumber;

        } while (!slice.IsEndOfStream);

        return events;
    }
}
