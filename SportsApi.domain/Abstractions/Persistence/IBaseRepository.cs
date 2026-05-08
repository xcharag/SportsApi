using System.Linq.Expressions;
using Ardalis.Specification;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Entities;
using SportsApi.domain.Abstractions.Specifications;

namespace SportsApi.domain.Abstractions.Persistence;

public interface IBaseRepository
{
}

public interface IBaseRepository<TEntity> : IBaseRepository where TEntity : Entity
{
    Task<TEntity?> GetBySpecificationAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken);
    Task<IEnumerable<TEntity>> GetListBySpecificationAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken);
    Task<Result<TEntity>> SaveAsync(TEntity newEntity, CancellationToken cancellationToken);
    Task<Result> SaveAsync(TEntity[] newEntities, CancellationToken cancellationToken);
    Task<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(TEntity[] entities, CancellationToken cancellationToken);
    Task<PaginationResult<TEntity>> GetPaginatedAsync<TPagination>(TPagination pagination, CancellationToken cancellationToken)
        where TPagination : PaginationSpecification<TEntity>;
    Task<Result> BulkUpsertAsync(TEntity[] entities, string pivot, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(TEntity entity, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(TEntity[] entities, CancellationToken cancellationToken);
    Task<int> CountBySpecificationAsync(Ardalis.Specification.ISpecification<TEntity> specification, bool includeInactive, CancellationToken cancellationToken);
    Task<decimal> AverageBySpecificationAsync(Ardalis.Specification.ISpecification<TEntity> specification, Expression<Func<TEntity, decimal>> selector, bool includeInactive, CancellationToken cancellationToken);
}