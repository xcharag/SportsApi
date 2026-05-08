using SportsApi.domain.Abstractions.Entities;

namespace SportsApi.domain.Abstractions.Persistence;

public interface IRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
}