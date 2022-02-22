using System;
using System.Text;
using Core.Events;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Core.EventStoreDB.Serialization;

public static class EventStoreDBSerializer
{
    public static T? Deserialize<T>(this ResolvedEvent resolvedEvent) where T : class =>
        Deserialize(resolvedEvent) as T;


    public static object? Deserialize(this ResolvedEvent resolvedEvent)
    {
        // get type
        var eventType = EventTypeMapper.ToType(resolvedEvent.Event.EventType);

        return eventType != null
            // deserialize event
            ? JsonConvert.DeserializeObject(Encoding.UTF8.GetString(resolvedEvent.Event.Data), eventType)
            : null;
    }

    public static EventData ToJsonEventData(this object @event) =>
        new(
            Guid.NewGuid(),
            EventTypeMapper.ToName(@event.GetType()),
            true,
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { }))
        );

public static EventData ToJsonEventData(
    object @event,
    string eventType,
    object? metadata = null
) =>
    new EventData(
        Guid.NewGuid(),
        eventType,
        true,
        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event)),
        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(metadata ?? new { }))
    );
}
