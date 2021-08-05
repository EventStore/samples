using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Queries;
using Nest;

namespace Carts.Carts.GettingCarts
{
    public class GetCarts
    {
        public int PageNumber { get; }
        public int PageSize { get; }

        private GetCarts(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static GetCarts Create(int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageNumber));
            if (pageSize is <= 0 or > 100)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            return new GetCarts(pageNumber, pageSize);
        }
    }

    internal class HandleGetCarts :
        IQueryHandler<GetCarts, IReadOnlyList<CartShortInfo>>
    {
        private readonly IElasticClient elasticClient;

        public HandleGetCarts(IElasticClient elasticClient)
        {
            this.elasticClient = elasticClient;
        }

        public async Task<IReadOnlyList<CartShortInfo>> Handle(GetCarts request,
            CancellationToken cancellationToken)
        {
            var result = await elasticClient.SearchAsync<CartShortInfo>(
                s => s
                    .Skip(request.PageNumber * request.PageSize)
                    .Take(request.PageSize),
                cancellationToken);

            return result.Documents.ToList();
        }
    }
}
