using System.Threading;
using System.Threading.Tasks;

namespace Core.Commands
{
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        Task Handle(TCommand command, CancellationToken ct);
    }
}
