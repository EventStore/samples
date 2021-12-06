using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Commands;

public class CommandBus: ICommandBus
{
    private readonly IServiceProvider serviceProvider;

    public CommandBus(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public Task Send<TCommand>(TCommand command, CancellationToken ct)
    {
        var commandHandler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        return commandHandler.Handle(command, ct);
    }
}