using System.Threading;
using System.Threading.Tasks;

namespace Core.Commands
{
    public interface ICommandBus
    {
        Task Send<TCommand>(TCommand command, CancellationToken ct) where TCommand : ICommand;
    }
}
