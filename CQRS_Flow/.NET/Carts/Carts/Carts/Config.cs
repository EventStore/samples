using System.Collections.Generic;
using Carts.Carts.AddingProduct;
using Carts.Carts.ConfirmingCart;
using Carts.Carts.GettingCartAtVersion;
using Carts.Carts.GettingCartById;
using Carts.Carts.GettingCartHistory;
using Carts.Carts.GettingCarts;
using Carts.Carts.InitializingCart;
using Carts.Carts.RemovingProduct;
using Carts.Pricing;
using Core.Commands;
using Core.ElasticSearch.Projections;
using Core.EventStoreDB.Repository;
using Core.Queries;
using Core.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Carts.Carts;

internal static class CartsConfig
{
    internal static void AddCarts(this IServiceCollection services)
    {
        services.AddScoped<IProductPriceCalculator, RandomProductPriceCalculator>()
            .AddScoped<IRepository<Cart>, EventStoreDBRepository<Cart>>();

        AddCommandHandlers(services);
        AddProjections(services);
        AddQueryHandlers(services);
    }

    private static void AddCommandHandlers(IServiceCollection services)
    {
        services.AddCommandHandler<InitializeCart, HandleInitializeCart>()
            .AddCommandHandler<AddProduct, HandleAddProduct>()
            .AddCommandHandler<RemoveProduct, HandleRemoveProduct>()
            .AddCommandHandler<ConfirmCart, HandleConfirmCart>();
    }

    private static void AddProjections(IServiceCollection services)
    {
        services
            .Project<CartInitialized, CartDetails>(@event => @event.CartId.ToString())
            .Project<ProductAdded, CartDetails>(@event => @event.CartId.ToString())
            .Project<ProductRemoved, CartDetails>(@event => @event.CartId.ToString())
            .Project<CartConfirmed, CartDetails>(@event => @event.CartId.ToString());

        services
            .Project<CartInitialized, CartShortInfo>(@event => @event.CartId.ToString())
            .Project<ProductAdded, CartShortInfo>(@event => @event.CartId.ToString())
            .Project<ProductRemoved, CartShortInfo>(@event => @event.CartId.ToString())
            .Project<CartConfirmed, CartShortInfo>(@event => @event.CartId.ToString());

        services
            .Project<CartInitialized, CartHistory>(@event => @event.CartId.ToString())
            .Project<ProductAdded, CartHistory>(@event => @event.CartId.ToString())
            .Project<ProductRemoved, CartHistory>(@event => @event.CartId.ToString())
            .Project<CartConfirmed, CartHistory>(@event => @event.CartId.ToString());
    }

    private static void AddQueryHandlers(IServiceCollection services)
    {
        services.AddQueryHandler<GetCartById, CartDetails, HandleGetCartById>()
            .AddQueryHandler<GetCarts, IReadOnlyList<CartShortInfo>, HandleGetCarts>()
            .AddQueryHandler<GetCartHistory, IReadOnlyList<CartHistory>, HandleGetCartHistory>()
            .AddQueryHandler<GetCartAtVersion, CartDetails, HandleGetCartAtVersion>();
    }
}