using System.Threading;
using System.Threading.Tasks;

namespace Core.Queries
{
    public interface IQueryHandler<in TQuery, TResponse>
    {
        Task<TResponse> Handle(TQuery query, CancellationToken ct);
    }
}
