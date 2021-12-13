using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.EventStoreDB.Serialization;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;

namespace Core.EventStoreDB.Subscriptions;

public record CheckpointStored(string SubscriptionId, long? Position, DateTime CheckpointedAt);

public class EventStoreDBSubscriptionCheckpointRepository: ISubscriptionCheckpointRepository
{
    private readonly IEventStoreConnection eventStoreClient;

    public EventStoreDBSubscriptionCheckpointRepository(
        Func<IEventStoreConnection> connectToEventStore
    )
    {
        this.eventStoreClient = (connectToEventStore ?? throw new ArgumentNullException(nameof(connectToEventStore)))();
    }

    public async ValueTask<long?> Load(string subscriptionId, CancellationToken ct)
    {
        var streamName = GetCheckpointStreamName(subscriptionId);

        var readResult = await eventStoreClient.ReadStreamEventsBackwardAsync(
            streamName,
            StreamPosition.End,
            1,
            false
        );

        if (readResult.Status != SliceReadStatus.Success)
            return null;

        ResolvedEvent? @event = readResult.Events.FirstOrDefault();

        return @event?.Deserialize<CheckpointStored>()!.Position;
    }

    public async ValueTask Store(string subscriptionId, long position, CancellationToken ct)
    {
        var @event = new CheckpointStored(subscriptionId, position, DateTime.UtcNow);
        var eventToAppend = new[] {@event.ToJsonEventData()};
        var streamName = GetCheckpointStreamName(subscriptionId);

        try
        {
            // store new checkpoint expecting stream to exist
            await eventStoreClient.AppendToStreamAsync(
                streamName,
                ExpectedVersion.StreamExists,
                eventToAppend
            );
        }
        catch (WrongExpectedVersionException)
        {
            // WrongExpectedVersionException means that stream did not exist
            // Set the checkpoint stream to have at most 1 event
            // using stream metadata $maxCount property
            await eventStoreClient.SetStreamMetadataAsync(
                streamName,
                ExpectedVersion.NoStream,
                StreamMetadata.Create(1)
            );

            // append event again expecting stream to not exist
            await eventStoreClient.AppendToStreamAsync(
                streamName,
                ExpectedVersion.NoStream,
                eventToAppend
            );
        }
    }

    private static string GetCheckpointStreamName(string subscriptionId) => $"checkpoint_{subscriptionId}";
}
