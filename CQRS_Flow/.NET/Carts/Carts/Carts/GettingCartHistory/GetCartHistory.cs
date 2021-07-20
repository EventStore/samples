using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using Nest;

namespace Carts.Carts.GettingCartHistory
{
    public class GetCartHistory: IQuery<IReadOnlyList<CartHistory>>
    {
        public Guid CartId { get; }
        public int PageNumber { get; }
        public int PageSize { get; }

        private GetCartHistory(Guid cartId, int pageNumber, int pageSize)
        {
            CartId = cartId;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static GetCartHistory Create(Guid cartId, int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize is <= 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            return new GetCartHistory(cartId, pageNumber, pageSize);
        }
    }

    internal class HandleGetCartHistory:
        IQueryHandler<GetCartHistory, IReadOnlyList<CartHistory>>
    {
        private readonly IElasticClient elasticClient;

        public HandleGetCartHistory(IElasticClient elasticClient)
        {
            this.elasticClient = elasticClient;
        }

        public async Task<IReadOnlyList<CartHistory>> Handle(GetCartHistory request,
            CancellationToken cancellationToken)
        {
            var result = await elasticClient.SearchAsync<CartHistory>(
                s => s
                    .Query(
                        q => q.Term(x => x.CartId, request.CartId)
                    )
                    .Skip(request.PageNumber * request.PageSize)
                    .Take(request.PageSize),
                cancellationToken);

            return result.Documents.ToList();
        }
    }
}
