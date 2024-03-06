using System;
using System.Threading;
using System.Threading.Tasks;
using Carts.Carts.GettingCartById;
using Core.EventStoreDB.Events;
using Core.Exceptions;
using Core.Queries;
using EventStore.Client;

namespace Carts.Carts.GettingCartAtVersion;

public class GetCartAtVersion
{
    public Guid CartId { get; }
    public ulong Version { get; }

    private GetCartAtVersion(Guid cartId, ulong version)
    {
        CartId = cartId;
        Version = version;
    }

    public static GetCartAtVersion Create(Guid cartId, ulong version)
    {
        if (cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new GetCartAtVersion(cartId, version);
    }
}

internal class HandleGetCartAtVersion :
    IQueryHandler<GetCartAtVersion, CartDetails>
{
    private readonly EventStoreClient eventStore;

    public HandleGetCartAtVersion(EventStoreClient eventStore)
    {
        this.eventStore = eventStore;
    }

    public async Task<CartDetails> Handle(GetCartAtVersion request, CancellationToken cancellationToken)
    {
        var cart = await eventStore.AggregateStream<CartDetails>(
            request.CartId,
            cancellationToken,
            request.Version
        );

        if (cart == null)
            throw AggregateNotFoundException.For<Cart>(request.CartId);

        return cart;
    }
}