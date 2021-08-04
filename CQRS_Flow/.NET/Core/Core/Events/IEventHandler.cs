using System.Threading;
using System.Threading.Tasks;

namespace Core.Events
{
    public interface IEventHandler<in TEvent>
           where TEvent : IEvent
    {
        Task Handle(TEvent @event, CancellationToken ct);
    }
}
