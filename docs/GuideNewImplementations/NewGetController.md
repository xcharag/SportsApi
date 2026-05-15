# Guide — Creating a GET Endpoint

This guide shows you how to add a **read** endpoint to the API.  
Two flavours are covered:

| Variant | HTTP | Use-case |
|---|---|---|
| **Get by ID** | `GET /api/v1/{resource}/{id}` | single record |
| **Get all (paginated)** | `GET /api/v1/{resource}` | paginated list with optional filters |

The Tournaments module is used as the working reference throughout.

---

## Folder layout

```
SportsApi.application/Modules/{Domain}/{Entity}/
├── Filters/
│   ├── {Entity}ByIdFilter.cs          ← always needed
│   └── All{Entity}sFilter.cs          ← needed for list
└── Queries/
    ├── Get{Entity}ById/
    │   ├── {Entity}ByIdQuery.cs
    │   ├── {Entity}ByIdQueryHandler.cs
    │   └── {Entity}ByIdQueryResult.cs
    └── GetAll{Entity}s/
        ├── All{Entity}sQuery.cs
        ├── All{Entity}sQueryHandler.cs
        └── All{Entity}sQueryResult.cs

SportsApi.api/Controllers/{Domain}/{Entity}/
├── Get{Entity}ById.cs
└── GetAll{Entity}s.cs
```

---

## Step 1 — Create the Specification/Filter

Filters live in `SportsApi.application` and are plain Ardalis `Specification<TEntity>` subclasses.

### Single-record filter

```csharp
// SportsApi.application/Modules/Tournaments/Tournaments/Filters/TournamentByIdFilter.cs
using Ardalis.Specification;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Filters;

public class TournamentByIdFilter : Specification<Tournament>
{
    public TournamentByIdFilter(Guid id)
    {
        Query.Where(t => t.Id == id);
    }
}
```

> **Tip:** Do not call `.Include(...)` in an ID filter unless you specifically need navigation properties — keep it minimal.

### Paginated filter

Extend `PaginationSpecification<TEntity>` (from `SportsApi.domain.Abstractions.Specifications`).  
The base class stores `Page` and `PerPage` so the repository can handle the skip/take automatically.

```csharp
// SportsApi.application/Modules/Tournaments/Tournaments/Filters/AllTournamentsFilter.cs
using Ardalis.Specification;
using SportsApi.domain.Abstractions.Specifications;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Filters;

public sealed class AllTournamentsFilter : PaginationSpecification<Tournament>
{
    public AllTournamentsFilter(
        int page,
        int perPage,
        string? name,
        DateTime? initDate,
        DateTime? endDate) : base(page, perPage)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.ToLower();
            Query.Where(t => t.Name.Contains(term, StringComparison.CurrentCultureIgnoreCase));
        }

        if (initDate.HasValue)
            Query.Where(t => t.StartDate >= initDate.Value);
        if (endDate.HasValue)
            Query.Where(t => t.EndDate <= endDate.Value);

        Query.OrderBy(t => t.StartDate);
        Query.Include(t => t.TeamsParticipations);
    }
}
```

---

## Step 2 — Query class

Implement `IQuery<TResult>`. For a list, add pagination + filter fields.

### Get by ID

```csharp
// Queries/GetTournamentById/TournamentByIdQuery.cs
using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentById;

public class TournamentByIdQuery : IQuery<TournamentByIdQueryResult>
{
    public Guid Id { get; set; }
}
```

### Get all (paginated)

```csharp
// Queries/GetAllTournaments/AllTournamentsQuery.cs
using SportsApi.domain.Abstractions.Messaging.Queries;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetAllTournaments;

public class AllTournamentsQuery : IQuery<AllTournamentsQueryResult>
{
    public int Page { get; set; }
    public int PerPage { get; set; }
    public string? Name { get; set; }
    public DateTime? InitDate { get; set; }
    public DateTime? EndDate { get; set; }
}
```

---

## Step 3 — Result class

Implement `IQueryResult`. Return only what the consumer needs.

### Get by ID

```csharp
// Queries/GetTournamentById/TournamentByIdQueryResult.cs
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentById;

public class TournamentByIdQueryResult : IQueryResult
{
    public required Tournament Data { get; set; }
}
```

> You may map to a DTO instead of returning the entity directly to avoid over-exposure.

### Get all (paginated)

```csharp
// Queries/GetAllTournaments/AllTournamentsQueryResult.cs
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetAllTournaments;

public class AllTournamentsQueryResult : IQueryResult
{
    public required PaginationResult<Tournament> Data { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public int TotalCount { get; set; }
}
```

`PaginationResult<T>` already contains `Page`, `PerPage`, `Count`, `TotalPages`, and `Data`.

---

## Step 4 — Query Handler

Implement `IQueryHandler<TQuery, TResult>`. Inject `IRepository<TEntity>`.  
**Never** inject `ICurrentUser` in a pure read — only inject what you actually need.

### Get by ID

```csharp
// Queries/GetTournamentById/TournamentByIdQueryHandler.cs
using SportsApi.application.Modules.Tournaments.Tournaments.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentById;

public class TournamentByIdQueryHandler(IRepository<Tournament> repository)
    : IQueryHandler<TournamentByIdQuery, TournamentByIdQueryResult>
{
    public async Task<Result<TournamentByIdQueryResult>> HandleAsync(
        TournamentByIdQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new TournamentByIdFilter(query.Id);
        var entity = await repository.GetBySpecificationAsync(filter, cancellationToken);

        if (entity is null)
            return Result.Fail<TournamentByIdQueryResult>("Tournament not found", "TOURNAMENT_NOT_FOUND");

        return Result.Success(new TournamentByIdQueryResult { Data = entity });
    }
}
```

### Get all (paginated)

```csharp
// Queries/GetAllTournaments/AllTournamentsQueryHandler.cs
using SportsApi.application.Modules.Tournaments.Tournaments.Filters;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetAllTournaments;

public class AllTournamentsQueryHandler(IRepository<Tournament> repository)
    : IQueryHandler<AllTournamentsQuery, AllTournamentsQueryResult>
{
    public async Task<Result<AllTournamentsQueryResult>> HandleAsync(
        AllTournamentsQuery query,
        CancellationToken cancellationToken)
    {
        var filter = new AllTournamentsFilter(
            query.Page, query.PerPage,
            query.Name, query.InitDate, query.EndDate);

        var paginatedResult = await repository.GetPaginatedAsync(filter, cancellationToken);

        var totalCount  = await repository.CountBySpecificationAsync(filter, includeInactive: true,  cancellationToken);
        var activeCount = await repository.CountBySpecificationAsync(filter, includeInactive: false, cancellationToken);

        return Result.Success(new AllTournamentsQueryResult
        {
            Data          = paginatedResult,
            TotalCount    = totalCount,
            ActiveCount   = activeCount,
            InactiveCount = totalCount - activeCount
        });
    }
}
```

---

## Step 5 — API Controller

Each endpoint is a standalone class inheriting from Ardalis `EndpointBaseAsync`.

### Get by ID controller

```csharp
// SportsApi.api/Controllers/Tournaments/Tournaments/GetTournamentById.cs
using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentById;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetTournamentById(IMediator mediator) : EndpointBaseAsync
    .WithRequest<TournamentByIdQuery>
    .WithActionResult<TournamentByIdQueryResult>
{
    [HttpGet("api/v1/tournaments/{id}", Name = "GetTournamentById")]
    [SwaggerOperation(Tags = ["Tournaments"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(TournamentByIdQueryResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<TournamentByIdQueryResult>> HandleAsync(
        [FromRoute] TournamentByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<TournamentByIdQuery, TournamentByIdQueryResult>(
            new TournamentByIdQuery { Id = request.Id }, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}
```

> The `Name = "GetTournamentById"` on the route is used by POST endpoints that call `CreatedAtRoute(...)`.

### Get all controller

```csharp
// SportsApi.api/Controllers/Tournaments/Tournaments/GetAllTournaments.cs
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetAllTournaments;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetAllTournaments(IMediator mediator) : EndpointBaseAsync
    .WithRequest<AllTournamentsQuery>
    .WithActionResult<AllTournamentsQueryResult>
{
    [HttpGet("api/v1/tournaments")]
    [SwaggerOperation(Tags = ["Tournaments"])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AllTournamentsQueryResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public override async Task<ActionResult<AllTournamentsQueryResult>> HandleAsync(
        [FromQuery] AllTournamentsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<AllTournamentsQuery, AllTournamentsQueryResult>(
            request, cancellationToken);

        if (result.IsFailure)
            return Problem(
                title: "Failed to retrieve tournaments",
                detail: result.Error,
                statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}
```

> Bind a list query with `[FromQuery]`. Bind a by-ID query with `[FromRoute]`.

---

## Checklist

- [ ] Filter created in `Filters/`
- [ ] Query class implements `IQuery<TResult>`
- [ ] Result class implements `IQueryResult`
- [ ] Handler implements `IQueryHandler<TQuery, TResult>` and registered automatically via assembly scan
- [ ] Controller uses `[ApiController]`, `[Authorize]`, `[DynamicPermission]`
- [ ] `[SwaggerOperation(Tags = ["..."])]` set so DynamicPermission resolves the resource name
- [ ] Route named (`Name = "Get{Entity}ById"`) if a POST endpoint needs `CreatedAtRoute`
- [ ] `[ProducesResponseType]` attributes added for Swagger accuracy

---

## Quick-reference: Repository read methods

| Method | When to use |
|---|---|
| `GetBySpecificationAsync` | Single active record |
| `GetListBySpecificationAsync` | Multiple active records (no pagination) |
| `GetBySpecificationAnyStateAsync` | Single record regardless of `Active` flag |
| `GetPaginatedAsync<TPagination>` | Paginated active records |
| `CountBySpecificationAsync` | Count for statistics (pass `includeInactive` flag) |

