namespace SportsApi.domain.Abstractions.Persistence;

public interface ICoreUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}