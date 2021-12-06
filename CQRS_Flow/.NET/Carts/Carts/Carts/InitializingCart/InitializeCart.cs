using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Repositories;

namespace Carts.Carts.InitializingCart;

public class InitializeCart
{
    public Guid CartId { get; }

    public Guid ClientId { get; }

    private InitializeCart(Guid cartId, Guid clientId)
    {
        CartId = cartId;
        ClientId = clientId;
    }

    public static InitializeCart Create(Guid? cartId, Guid? clientId)
    {
        if (!cartId.HasValue|| cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (!clientId.HasValue || clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));

        return new InitializeCart(cartId.Value, clientId.Value);
    }
}

internal class HandleInitializeCart:
    ICommandHandler<InitializeCart>
{
    private readonly IRepository<Cart> cartRepository;

    public HandleInitializeCart(
        IRepository<Cart> cartRepository
    )
    {
        this.cartRepository = cartRepository;
    }

    public async Task Handle(InitializeCart command, CancellationToken cancellationToken)
    {
        var cart = Cart.Initialize(command.CartId, command.ClientId);

        await cartRepository.Add(cart, cancellationToken);
    }
}