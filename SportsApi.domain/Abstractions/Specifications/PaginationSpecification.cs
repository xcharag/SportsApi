using Ardalis.Specification;
using SportsApi.domain.Abstractions.Entities;

namespace SportsApi.domain.Abstractions.Specifications;

public abstract class PaginationSpecification<TEntity>(int page, int perPage) : Specification<TEntity>
    where TEntity : Entity
{
    public int Page { get; } = page;
    public int PerPage { get; } = perPage;
}