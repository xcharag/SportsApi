# Guide — Creating a DELETE Endpoint

This guide shows you how to add a **delete** endpoint to the API.

The system supports two deletion modes:

| Mode | `HardDelete` | What happens |
|---|---|---|
| **Soft delete** (default) | `false` | Sets `Active = false`, `DeletedAt`, `DeletedBy`. Row stays in DB. `HasQueryFilter` hides it from all reads. |
| **Hard delete** | `true` | Physically removes the row with `repository.DeleteAsync`. |

The Tournaments module (`DeleteTournament`) is used as the working reference.

| HTTP | Route pattern | Response |
|---|---|---|
| `DELETE` | `/api/v1/{resource}/{id}` | `200 OK` with deleted item's `Id` |

---

## Folder layout

```
SportsApi.application/Modules/{Domain}/{Entity}/
├── Filters/
│   └── {Entity}ByIdFilter.cs          ← reuse the one from GET
└── Commands/
    └── Delete{Entity}/
        ├── Delete{Entity}Command.cs
        ├── Delete{Entity}CommandHandler.cs
        └── Delete{Entity}CommandResult.cs

SportsApi.api/Controllers/{Domain}/{Entity}/
└── Delete{Entity}.cs
```

---

## Step 1 — Command class

The command must carry the `Id` (from route) and optionally a `HardDelete` flag (from query string or body).

```csharp
// Commands/DeleteTournament/DeleteTournamentCommand.cs
using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.DeleteTournament;

public class DeleteTournamentCommand : ICommand<DeleteTournamentCommandResult>
{
    public Guid Id { get; set; }

    /// <summary>
    /// When true the row is physically removed (hard delete).
    /// When false (default) the entity is soft-deleted (Active = false).
    /// </summary>
    public bool HardDelete { get; set; } = false;
}
```

---

## Step 2 — Result class

Implement `ICommandResult`. Return at minimum the `Id` that was deleted.

```csharp
// Commands/DeleteTournament/DeleteTournamentCommandResult.cs
using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.DeleteTournament;

public class DeleteTournamentCommandResult : ICommandResult
{
    public Guid   Id   { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

---

## Step 3 — Command Handler

The pattern is: **fetch → guard → soft-delete OR hard-delete → save**.

```csharp
// Commands/DeleteTournament/DeleteTournamentCommandHandler.cs
using SportsApi.application.Modules.Tournaments.Tournaments.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.DeleteTournament;

public class DeleteTournamentCommandHandler(
    IRepository<Tournament> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<DeleteTournamentCommand, DeleteTournamentCommandResult>
{
    public async Task<Result<DeleteTournamentCommandResult>> HandleAsync(
        DeleteTournamentCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Fetch (active entities only)
        var filter = new TournamentByIdFilter(command.Id);
        var entity = await repository.GetBySpecificationAsync(filter, cancellationToken);

        // 2. Guard
        if (entity is null)
            return Result.Fail<DeleteTournamentCommandResult>(
                "Tournament not found", "TOURNAMENT_NOT_FOUND");

        var username = currentUser.Username;

        if (command.HardDelete)
        {
            // 3a. Hard delete — physically remove the row
            entity.DeletedAt = DateTime.Now;
            entity.DeletedBy = username;

            var deleteResult = await repository.DeleteAsync(entity, cancellationToken);
            if (deleteResult.IsFailure)
                return Result.Fail<DeleteTournamentCommandResult>(
                    deleteResult.Error, deleteResult.ErrorKey);
        }
        else
        {
            // 3b. Soft delete — mark inactive, keep the row
            entity.Active    = false;
            entity.DeletedAt = DateTime.Now;
            entity.DeletedBy = username;

            var updateResult = await repository.UpdateAsync(entity, cancellationToken);
            if (updateResult.IsFailure)
                return Result.Fail<DeleteTournamentCommandResult>(
                    updateResult.Error, updateResult.ErrorKey);
        }

        // 4. Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteTournamentCommandResult { Id = entity.Id });
    }
}
```

### Rules

| Concern | Rule |
|---|---|
| Fetch | Use `GetBySpecificationAsync` (respects the `Active` query filter — already-deleted records are not found) |
| Guard | Return `Result.Fail` with a clear `ErrorKey` if not found |
| Soft-delete | Set `Active = false`, `DeletedAt`, `DeletedBy`; call `UpdateAsync` |
| Hard-delete | Set `DeletedAt`, `DeletedBy` for the audit trail; call `DeleteAsync` |
| Audit | Always record who deleted the entity, regardless of mode |
| Persist | Single `unitOfWork.SaveChangesAsync` after the repository call |
| Error | Return `Result.Fail<T>(message, errorKey)` — never throw |

---

## Step 4 — API Controller

```csharp
// SportsApi.api/Controllers/Tournaments/Tournaments/DeleteTournament.cs
using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Commands.DeleteTournament;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[ApiController]
[Authorize]
[DynamicPermission]
public class DeleteTournament(IMediator mediator) : EndpointBaseAsync
    .WithRequest<DeleteTournamentCommand>
    .WithActionResult<DeleteTournamentCommandResult>
{
    [HttpDelete("api/v1/tournaments/{id}")]
    [SwaggerOperation(Tags = ["Tournaments"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(DeleteTournamentCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<DeleteTournamentCommandResult>> HandleAsync(
        [FromRoute] DeleteTournamentCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator
            .SendCommandAsync<DeleteTournamentCommand, DeleteTournamentCommandResult>(
                request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}
```

### Exposing `HardDelete` as a query-string flag

Binding `[FromRoute]` on the entire command model makes both `id` (route token) and `hardDelete`  
(query-string) bind automatically:

```
DELETE /api/v1/tournaments/3fa85f64-...           → soft delete (default)
DELETE /api/v1/tournaments/3fa85f64-...?hardDelete=true  → hard delete
```

No extra plumbing is required — ASP.NET Core's model binder handles both sources.

---

## Soft-delete vs Hard-delete decision guide

| Question | Answer |
|---|---|
| Should the row ever be auditable / restorable? | Use **soft delete** |
| Does a unique constraint block re-creation of a record with the same key? | Use **hard delete** |
| Is the data referenced by other tables with `ON DELETE RESTRICT`? | Soft delete is safer; consider cascade rules |
| Is this a pure admin purge operation? | Hard delete is acceptable |

---

## Checklist

- [ ] Reused the existing `{Entity}ByIdFilter` (not duplicated)
- [ ] Command implements `ICommand<TResult>`; includes `HardDelete` flag if both modes are needed
- [ ] Result implements `ICommandResult` and includes at least `Id`
- [ ] Handler sets `DeletedAt` and `DeletedBy` in **both** paths
- [ ] Soft-delete path: `Active = false` + `UpdateAsync`
- [ ] Hard-delete path: `DeleteAsync`
- [ ] Single `unitOfWork.SaveChangesAsync` after the repository call
- [ ] Handler returns `Result.Fail<T>(message, errorKey)` on errors
- [ ] Controller uses `[ApiController]`, `[Authorize]`, `[DynamicPermission]`
- [ ] Controller binds with `[FromRoute]` (allows `HardDelete` from query-string)
- [ ] Controller returns `NotFound` on failure, `Ok` on success
- [ ] `[SwaggerOperation(Tags)]` set
- [ ] `[ProducesResponseType]` for `200` and `404` added
- [ ] `DynamicPermission` maps DELETE → `"Delete"` automatically

