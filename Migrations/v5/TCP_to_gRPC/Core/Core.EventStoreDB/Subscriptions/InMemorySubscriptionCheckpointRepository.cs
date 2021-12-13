using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Core.EventStoreDB.Subscriptions;

public class InMemorySubscriptionCheckpointRepository: ISubscriptionCheckpointRepository
{
    private readonly ConcurrentDictionary<string, long> checkpoints = new();

    public ValueTask<long?> Load(string subscriptionId, CancellationToken ct)
    {
        return new(checkpoints.TryGetValue(subscriptionId, out var checkpoint) ? checkpoint : null);
    }

    public ValueTask Store(string subscriptionId, long position, CancellationToken ct)
    {
        checkpoints.AddOrUpdate(subscriptionId, position,(_, _) => position);

        return ValueTask.CompletedTask;
    }
}
