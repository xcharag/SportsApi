using System.Linq.Expressions;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Entities;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Abstractions.Specifications;

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework.Repositories;

public class EfRepository<TEntity>(CoreDbContext dbContext)
    : IRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> dbSet = dbContext.Set<TEntity>();

    public async Task<TEntity?> GetBySpecificationAsync(
        Ardalis.Specification.ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return await dbSet.AsQueryable()
            .WithSpecification(specification)
            .Where(x => x.Active)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetListBySpecificationAsync(
        Ardalis.Specification.ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return await dbSet.AsQueryable()
            .WithSpecification(specification)
            .Where(x => x.Active)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<TEntity?> GetBySpecificationAnyStateAsync(
        Ardalis.Specification.ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return await dbSet.AsQueryable()
            .IgnoreQueryFilters()
            .WithSpecification(specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetListBySpecificationAnyStateAsync(
        Ardalis.Specification.ISpecification<TEntity> specification,
        CancellationToken cancellationToken)
    {
        return await dbSet.AsQueryable()
            .IgnoreQueryFilters()
            .WithSpecification(specification)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Result<TEntity>> SaveAsync(TEntity newEntity, CancellationToken cancellationToken)
    {
        var entity = await dbSet.AddAsync(newEntity, cancellationToken);
        return Result.Success(entity.Entity);
    }

    public async Task<Result> SaveAsync(TEntity[] newEntities, CancellationToken cancellationToken)
    {
        await dbSet.AddRangeAsync(newEntities, cancellationToken);
        return Result.Success();
    }

    public Task<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        dbSet.Update(entity);
        return Task.FromResult(Result.Success(entity));
    }

    public Task<Result> UpdateAsync(TEntity[] entities, CancellationToken cancellationToken)
    {
        dbSet.UpdateRange(entities);
        return Task.FromResult(Result.Success());
    }

    public async Task<PaginationResult<TEntity>> GetPaginatedAsync<TPagination>(
        TPagination pagination,
        CancellationToken cancellationToken)
        where TPagination : PaginationSpecification<TEntity>
    {
        var queryable = dbSet.Where(x => x.Active).AsQueryable();

        var count = await queryable.WithSpecification(pagination).CountAsync(cancellationToken);
        var data = await queryable
            .WithSpecification(pagination)
            .Skip((pagination.Page - 1) * pagination.PerPage)
            .Take(pagination.PerPage)
            .ToArrayAsync(cancellationToken);

        return new PaginationResult<TEntity>
        {
            Page = pagination.Page,
            PerPage = pagination.PerPage,
            Count = count,
            TotalPages = count == 0 ? 0 : (int)Math.Ceiling((double)count / pagination.PerPage),
            Data = data
        };
    }

    public async Task<int> CountBySpecificationAsync(Ardalis.Specification.ISpecification<TEntity> specification, bool includeInactive, CancellationToken cancellationToken)
    {
        var queryable = includeInactive
            ? dbSet.AsQueryable().IgnoreQueryFilters()
            : dbSet.Where(x => x.Active).AsQueryable();
        return await queryable.WithSpecification(specification).CountAsync(cancellationToken);
    }

    public async Task<decimal> AverageBySpecificationAsync(Ardalis.Specification.ISpecification<TEntity> specification, Expression<Func<TEntity, decimal>> selector, bool includeInactive, CancellationToken cancellationToken)
    {
        var queryable = includeInactive
            ? dbSet.AsQueryable().IgnoreQueryFilters()
            : dbSet.Where(x => x.Active).AsQueryable();
        var q = queryable.WithSpecification(specification);
        var any = await q.AnyAsync(cancellationToken);
        if (!any) return 0m;
        return await q.AverageAsync(selector, cancellationToken);
    }

    public Task<Result> BulkUpsertAsync(TEntity[] entities, string pivot, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        dbSet.Remove(entity);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> DeleteAsync(TEntity[] entities, CancellationToken cancellationToken)
    {
        dbSet.RemoveRange(entities);
        return Task.FromResult(Result.Success());
    }

}