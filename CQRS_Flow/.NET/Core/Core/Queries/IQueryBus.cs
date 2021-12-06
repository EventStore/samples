using System.Threading;
using System.Threading.Tasks;

namespace Core.Queries;

public interface IQueryBus
{
    Task<TResponse> Send<TQuery, TResponse>(TQuery query, CancellationToken ct);
}