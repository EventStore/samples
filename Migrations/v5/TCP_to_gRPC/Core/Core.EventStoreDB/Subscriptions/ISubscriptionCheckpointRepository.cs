using System.Threading;
using System.Threading.Tasks;

namespace Core.EventStoreDB.Subscriptions;

public interface ISubscriptionCheckpointRepository
{
    ValueTask<long?> Load(string subscriptionId, CancellationToken ct);

    ValueTask Store(string subscriptionId, long position, CancellationToken ct);
}
