using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.EntityFrameworkCore.Storage;
using SportsApi.domain.Abstractions.Exceptions;
using SportsApi.domain.Abstractions.Persistence;

namespace SportsApi.infrastructure.Persistence.Core.EntityFramework;

public class EfUnitOfWork(CoreDbContext dbContext) : ICoreUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await operation();
            await transaction.CommitAsync(cancellationToken);
        });
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (TryMapUniqueConstraint(ex, out var mapped))
        {
            throw mapped;
        }
    }

    private static bool TryMapUniqueConstraint(DbUpdateException ex, out DomainConflictException mapped)
    {
        if (ex.InnerException is not PostgresException pgEx || pgEx.SqlState != PostgresErrorCodes.UniqueViolation)
        {
            mapped = default!;
            return false;
        }

        var constraintName = pgEx.ConstraintName ?? string.Empty;
        foreach (var rule in UniqueRules)
        {
            if (!constraintName.Contains(rule.IndexOrConstraint, StringComparison.OrdinalIgnoreCase))
                continue;

            mapped = new DomainConflictException(rule.Message, rule.ErrorKey);
            return true;
        }

        mapped = new DomainConflictException(
            "El registro viola una restriccion de unicidad. Verifique los datos duplicados.",
            "UNIQUE_CONSTRAINT_VIOLATION");
        return true;
    }

    private static readonly (string IndexOrConstraint, string Message, string ErrorKey)[] UniqueRules =
    [
        
    ];

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}