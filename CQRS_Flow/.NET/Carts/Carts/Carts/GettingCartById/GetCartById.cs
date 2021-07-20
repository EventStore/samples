using System;
using System.Threading;
using System.Threading.Tasks;
using Core.ElasticSearch.Indices;
using Core.Exceptions;
using Core.Queries;
using Nest;

namespace Carts.Carts.GettingCartById
{
    public class GetCartById : IQuery<CartDetails>
    {
        public Guid CartId { get; }

        private GetCartById(Guid cartId)
        {
            CartId = cartId;
        }

        public static GetCartById Create(Guid cartId)
        {
            if (cartId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cartId));

            return new GetCartById(cartId);
        }
    }

    internal class HandleGetCartById :
        IQueryHandler<GetCartById, CartDetails>
    {
        private readonly IElasticClient elasticClient;

        public HandleGetCartById(IElasticClient elasticClient)
        {
            this.elasticClient = elasticClient;
        }

        public async Task<CartDetails> Handle(GetCartById request, CancellationToken cancellationToken)
        {
            var result = await elasticClient.GetAsync<CartDetails>(request.CartId,
                c => c.Index(IndexNameMapper.ToIndexName<CartDetails>()),
                ct: cancellationToken);

            return result?.Source ?? throw AggregateNotFoundException.For<Cart>(request.CartId);
        }
    }
}
