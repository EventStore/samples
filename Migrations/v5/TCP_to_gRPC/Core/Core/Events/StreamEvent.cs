namespace Core.Events;

public record EventMetadata(
    long StreamRevision
);

public class StreamEvent
{
    public object Data { get; }
    public EventMetadata Metadata { get; }

    public StreamEvent(object data, EventMetadata metadata)
    {
        Data = data;
        Metadata = metadata;
    }
}

public class StreamEvent<T>: StreamEvent where T: notnull
{
    public new T Data => (T)base.Data;

    public StreamEvent(T data, EventMetadata metadata) : base(data, metadata)
    {
    }
}

