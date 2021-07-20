using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Carts.Api.Requests.Carts;
using Carts.Carts.GettingCartAtVersion;
using Carts.Carts.GettingCartById;
using Carts.Carts.GettingCartHistory;
using Carts.Carts.GettingCarts;
using Carts.Carts.Products;
using Microsoft.AspNetCore.Mvc;
using Core.Commands;
using Core.Ids;
using Core.Queries;

namespace Carts.Api.Controllers
{
    [Route("api/[controller]")]
    public class CartsController: Controller
    {
        private readonly ICommandBus commandBus;
        private readonly IQueryBus queryBus;
        private readonly IIdGenerator idGenerator;

        public CartsController(
            ICommandBus commandBus,
            IQueryBus queryBus,
            IIdGenerator idGenerator)
        {
            this.commandBus = commandBus;
            this.queryBus = queryBus;
            this.idGenerator = idGenerator;
        }

        [HttpPost]
        public async Task<IActionResult> InitializeCart([FromBody] InitializeCartRequest? request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var cartId = idGenerator.New();

            var command = Carts.InitializingCart.InitializeCart.Create(
                cartId,
                request.ClientId
            );

            await commandBus.Send(command);

            return Created("api/Carts", cartId);
        }

        [HttpPost("{id}/products")]
        public async Task<IActionResult> AddProduct(Guid id, [FromBody] AddProductRequest? request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = Carts.AddingProduct.AddProduct.Create(
                id,
                ProductItem.Create(
                    request.ProductItem?.ProductId,
                    request.ProductItem?.Quantity
                )
            );

            await commandBus.Send(command);

            return Ok();
        }

        [HttpDelete("{id}/products")]
        public async Task<IActionResult> RemoveProduct(Guid id, [FromBody] RemoveProductRequest? request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var command = Carts.RemovingProduct.RemoveProduct.Create(
                id,
                PricedProductItem.Create(
                    request.ProductItem?.ProductId,
                    request.ProductItem?.Quantity,
                    request.ProductItem?.UnitPrice
                )
            );

            await commandBus.Send(command);

            return Ok();
        }

        [HttpPut("{id}/confirmation")]
        public async Task<IActionResult> ConfirmCart(Guid id)
        {
            var command = Carts.ConfirmingCart.ConfirmCart.Create(
                id
            );

            await commandBus.Send(command);

            return Ok();
        }

        [HttpGet("{id}")]
        public Task<CartDetails> Get(Guid id)
        {
            return queryBus.Send<GetCartById, CartDetails>(GetCartById.Create(id));
        }

        [HttpGet]
        public Task<IReadOnlyList<CartShortInfo>> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            return queryBus.Send<GetCarts, IReadOnlyList<CartShortInfo>>(GetCarts.Create(pageNumber, pageSize));
        }


        [HttpGet("{id}/history")]
        public Task<IReadOnlyList<CartHistory>> GetHistory(Guid id)
        {
            return queryBus.Send<GetCartHistory, IReadOnlyList<CartHistory>>(GetCartHistory.Create(id));
        }

        [HttpGet("{id}/versions")]
        public Task<CartDetails> GetVersion(Guid id, [FromQuery] GetCartAtVersion? query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            return queryBus.Send<GetCartAtVersion, CartDetails>(GetCartAtVersion.Create(id, query.Version));
        }
    }
}
