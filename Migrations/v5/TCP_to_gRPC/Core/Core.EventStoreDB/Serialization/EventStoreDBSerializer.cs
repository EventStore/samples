using System;
using System.Text;
using Core.Events;
using Newtonsoft.Json;

namespace Core.EventStoreDB.Serialization;

public static class EventStoreDBSerializer
{
    // GRPC
    public static T? Deserialize<T>(this EventStore.Client.ResolvedEvent resolvedEvent) where T : class =>
        Deserialize(resolvedEvent) as T;

    public static object? Deserialize(this EventStore.Client.ResolvedEvent resolvedEvent)
    {
        // get type
        var eventType = EventTypeMapper.ToType(resolvedEvent.Event.EventType);

        return eventType != null
            // deserialize event
            ? JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span), eventType)
            : null;
    }

    public static EventStore.Client.EventData ToGrpcJsonEventData(this object @event) =>
        new(
            EventStore.Client.Uuid.NewUuid(),
            EventTypeMapper.ToName(@event.GetType()),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { }))
        );

    // TCP
    public static T? Deserialize<T>(this EventStore.ClientAPI.ResolvedEvent resolvedEvent) where T : class =>
        Deserialize(resolvedEvent) as T;


    public static object? Deserialize(this EventStore.ClientAPI.ResolvedEvent resolvedEvent)
    {
        // get type
        var eventType = EventTypeMapper.ToType(resolvedEvent.Event.EventType);

        return eventType != null
            // deserialize event
            ? JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Data), eventType)
            : null;
    }

    public static EventStore.ClientAPI.EventData ToJsonEventData(this object @event) =>
        new(
            Guid.NewGuid(),
            EventTypeMapper.ToName(@event.GetType()),
            true,
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { }))
        );
}
