# Core Patterns

This document describes the architectural patterns used throughout the SportsApi solution.

---

## 1. Result Pattern

All operations return a `Result` or `Result<T>` instead of throwing exceptions for expected failures.

```csharp
// Non-generic (void operations)
Result.Success()
Result.Fail("Something went wrong", "ERROR_KEY")

// Generic (operations that return a value)
Result.Success<Tournament>(entity)
Result.Fail<Tournament>("Not found", "NOT_FOUND")
```

### Checking a result
```csharp
var result = await repository.SaveAsync(entity, ct);
if (result.IsFailure)
    return Result.Fail<MyCommandResult>(result.Error);

var value = result.Value; // safe, IsSuccess == true
```

`Result` exposes:
- `IsSuccess` / `IsFailure`
- `Error` — human-readable message
- `ErrorKey` — machine-readable code (useful for i18n)
- `Value` — only on `Result<T>`

---

## 2. CQRS via Custom Mediator

The project uses a **lightweight, custom mediator** (not MediatR) that resolves handlers from the DI container at runtime.

### Interfaces (defined in `SportsApi.domain`)

```
ICommand                           — fire-and-forget command (returns Result)
ICommand<TResult>                  — command returning a typed result
ICommandHandler<TCommand>          — handler for void command
ICommandHandler<TCommand, TResult> — handler for typed command
IQuery<TResult>                    — query returning a typed result
IQueryHandler<TQuery, TResult>     — handler for a query
```

### Sending via `IMediator`

```csharp
// Command with result
Result<MyResult> result = await mediator.SendCommandAsync<MyCommand, MyResult>(command, ct);

// Command without result
Result result = await mediator.SendCommandAsync<MyCommand>(command, ct);

// Query
Result<MyResult> result = await mediator.SendQueryAsync<MyQuery, MyResult>(query, ct);
```

### Auto-Registration

`ApplicationServiceExtensions.AddApplication()` uses reflection to scan the `SportsApi.application` assembly and register all `ICommandHandler<,>`, `ICommandHandler<>`, and `IQueryHandler<,>` implementations as **scoped** services automatically. No manual registration is needed.

---

## 3. Repository Pattern

### `IBaseRepository<TEntity>`
The generic data-access contract. Key methods:

| Method | Description |
|---|---|
| `GetBySpecificationAsync` | Fetch first active match for a specification |
| `GetListBySpecificationAsync` | Fetch all active matches |
| `GetBySpecificationAnyStateAsync` | Fetch first match, ignoring soft-delete filter |
| `GetListBySpecificationAnyStateAsync` | Fetch all, ignoring soft-delete filter |
| `SaveAsync(entity)` | Add single entity |
| `SaveAsync(entities[])` | Add multiple entities |
| `UpdateAsync(entity)` | Track entity as modified |
| `UpdateAsync(entities[])` | Track multiple entities as modified |
| `GetPaginatedAsync<TPagination>` | Paginated query with count |
| `DeleteAsync(entity)` | Hard-delete (remove from DB) |
| `DeleteAsync(entities[])` | Hard-delete multiple |
| `CountBySpecificationAsync` | Count with optional inactive inclusion |
| `AverageBySpecificationAsync` | Decimal average over a projection |

### `IRepository<TEntity>`
Extends `IBaseRepository<TEntity>` with constraint `TEntity : BaseEntity`. Use this in application handlers — do not use `IBaseRepository<T>` directly unless working with non-`BaseEntity` types.

### `EfRepository<TEntity>`
The EF Core implementation backed by `CoreDbContext`. All query methods apply the `x.Active` filter automatically. The `AnyState` variants call `.IgnoreQueryFilters()`.

---

## 4. Unit of Work

### `ICoreUnitOfWork`

```csharp
Task SaveChangesAsync(CancellationToken cancellationToken);
```

### `EfUnitOfWork`

Wraps `CoreDbContext.SaveChangesAsync` and translates `PostgresException` (SQLSTATE `23505`) into `DomainConflictException` using a lookup table of constraint names → messages.

Also provides low-level transaction control:
- `BeginTransactionAsync`
- `CommitTransactionAsync`
- `RollbackTransactionAsync`
- `ExecuteInTransactionAsync(Func<Task>)` — preferred: uses EF's execution strategy for automatic retries.

**Always call `unitOfWork.SaveChangesAsync()` at the end of a handler after all repository operations.**

---

## 5. Specification Pattern

Built on top of [Ardalis.Specification](https://github.com/ardalis/Specification).

### `PaginationSpecification<TEntity>`
Base class for paginated queries. All specifications that should support paging inherit from this.

```csharp
public class GetTournamentsSpec(int page, int perPage) 
    : PaginationSpecification<Tournament>(page, perPage)
{
    // Add Where/OrderBy/Include inside constructor via Query.Where(...)
}
```

Pass to `repository.GetPaginatedAsync(spec, ct)` and receive a `PaginationResult<TEntity>` containing `Page`, `PerPage`, `Count`, `TotalPages`, and `Data`.

---

## 6. Soft Delete

Soft deletes are enforced at the EF Core query-filter level (`BaseEntityConfiguration<T>`). Setting `Active = false` on an entity and calling `SaveChanges` is the deletion mechanism — **never use `DbSet.Remove`** for business-logic deletions. (Hard delete via `repository.DeleteAsync` exists for cascade/cleanup scenarios.)

---

## 7. Pagination

`PaginationResult<TEntity>` is the standard paging envelope:

```json
{
  "page": 1,
  "perPage": 20,
  "count": 45,
  "totalPages": 3,
  "data": [ ... ]
}
```

