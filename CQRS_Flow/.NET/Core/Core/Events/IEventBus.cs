using System.Threading;
using System.Threading.Tasks;

namespace Core.Events
{
    public interface IEventBus
    {
        Task Publish<TEvent>(TEvent @event, CancellationToken ct)
            where TEvent: IEvent;

        Task Publish(object @event, CancellationToken ct);
    }
}
