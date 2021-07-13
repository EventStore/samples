using System;
using System.Threading.Tasks;
using MediatR;

namespace Core.Events
{
    public class EventBus: IEventBus
    {
        private readonly IMediator mediator;

        public EventBus(
            IMediator mediator
        )
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task Publish(params IEvent[] events)
        {
            foreach (var @event in events)
            {
                await mediator.Publish(@event);
            }
        }
    }
}
