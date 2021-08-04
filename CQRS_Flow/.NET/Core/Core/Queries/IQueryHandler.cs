using System.Threading;
using System.Threading.Tasks;

namespace Core.Queries
{
    public interface IQueryHandler<in TQuery, TResponse>
           where TQuery : IQuery<TResponse>
    {
        Task<TResponse> Handle(TQuery query, CancellationToken ct);
    }
}
