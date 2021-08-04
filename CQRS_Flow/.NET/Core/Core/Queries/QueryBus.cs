using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Queries
{
    public class QueryBus: IQueryBus
    {
        private readonly IServiceProvider serviceProvider;

        public QueryBus(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public Task<TResponse> Send<TQuery, TResponse>(TQuery query, CancellationToken ct)
        {
            var queryHandler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
            return queryHandler.Handle(query, ct);
        }
    }
}
