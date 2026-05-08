using SportsApi.domain.Abstractions.Dtos;

namespace SportsApi.domain.Abstractions.Messaging.Queries;

public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken);
}