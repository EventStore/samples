using System.Threading;
using System.Threading.Tasks;

namespace Core.Events;

public interface IEventBus
{
    Task Publish(object @event, CancellationToken ct);
}
