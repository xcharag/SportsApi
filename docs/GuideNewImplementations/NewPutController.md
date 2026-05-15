# Guide — Creating a PUT Endpoint

This guide shows you how to add an **update** endpoint to the API.

| HTTP | Route pattern | Response |
|---|---|---|
| `PUT` | `/api/v1/{resource}/{id}` | `200 OK` with updated resource |

The Tournaments module (`PutUpdateTournament`) is used as the working reference.

---

## Folder layout

```
SportsApi.application/Modules/{Domain}/{Entity}/
├── Filters/
│   └── {Entity}ByIdFilter.cs      ← reuse the one from GET (do NOT duplicate)
└── Commands/
    └── Put{Update}{Entity}/
        ├── Update{Entity}Command.cs
        ├── Update{Entity}CommandHandler.cs
        └── Update{Entity}CommandResult.cs

SportsApi.api/Controllers/{Domain}/{Entity}/
└── Put{Update}{Entity}.cs
```

> **Reuse the filter.** The by-ID filter is already created for the GET endpoint — import and reuse it; do not create a second copy.

---

## Step 1 — Command class

Implement `ICommand<TResult>`.  
Make body fields **nullable** so the client only needs to send the fields it wants to change (partial update semantics).

```csharp
// Commands/PutUpdateTournament/UpdateTournamentCommand.cs
using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.PutUpdateTournament;

public class UpdateTournamentCommand : ICommand<UpdateTournamentCommandResult>
{
    public Guid    Id          { get; set; }          // comes from route
    public string? Name        { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate   { get; set; }
    public string? LogoUrl     { get; set; }
    public string? BannerUrl   { get; set; }
}
```

> `Id` is bound from the route; the rest are bound from the body.  
> Use `class` (not `record`) for update commands — mutable binding from multiple sources works more reliably with a plain class.

---

## Step 2 — Result class

Implement `ICommandResult`. Return only what the client needs after the update.

```csharp
// Commands/PutUpdateTournament/UpdateTournamentCommandResult.cs
using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.PutUpdateTournament;

public class UpdateTournamentCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}
```

---

## Step 3 — Command Handler

Implement `ICommandHandler<TCommand, TResult>`.  
The pattern is: **fetch → guard → patch → audit → save**.

```csharp
// Commands/PutUpdateTournament/UpdateTournamentCommandHandler.cs
using SportsApi.application.Modules.Tournaments.Tournaments.Filters;
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.PutUpdateTournament;

public class UpdateTournamentCommandHandler(
    IRepository<Tournament> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<UpdateTournamentCommand, UpdateTournamentCommandResult>
{
    public async Task<Result<UpdateTournamentCommandResult>> HandleAsync(
        UpdateTournamentCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Fetch
        var filter = new TournamentByIdFilter(command.Id);
        var entity = await repository.GetBySpecificationAsync(filter, cancellationToken);

        // 2. Guard
        if (entity is null)
            return Result.Fail<UpdateTournamentCommandResult>(
                "Tournament not found", "TOURNAMENT_NOT_FOUND");

        // 3. Patch — only update non-null fields
        if (command.Name        is not null) entity.Name        = command.Name;
        if (command.Description is not null) entity.Description = command.Description;
        if (command.StartDate   is not null) entity.StartDate   = command.StartDate.Value;
        if (command.EndDate     is not null) entity.EndDate     = command.EndDate.Value;
        if (command.LogoUrl     is not null) entity.LogoUrl     = command.LogoUrl;
        if (command.BannerUrl   is not null) entity.BannerUrl   = command.BannerUrl;

        // 4. Audit
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = currentUser.Username;

        // 5. Save
        var updateResult = await repository.UpdateAsync(entity, cancellationToken);
        if (updateResult.IsFailure)
            return Result.Fail<UpdateTournamentCommandResult>(
                $"Failed to update tournament: {updateResult.Error}", updateResult.ErrorKey);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateTournamentCommandResult { Id = entity.Id });
    }
}
```

### Rules

| Concern | Rule |
|---|---|
| Fetch first | Always load the entity from the DB — never build a detached entity to attach |
| Guard | If `null`, return `Result.Fail` with a descriptive `ErrorKey` |
| Patch | Check each nullable field before assigning — this avoids accidental overwrites with empty strings |
| Audit | Set `UpdatedAt = DateTime.Now` and `UpdatedBy = currentUser.Username` |
| Persist | `UpdateAsync` → `SaveChangesAsync` |
| Error | Return `Result.Fail<T>(message, errorKey)` — never throw |

---

## Step 4 — API Controller

```csharp
// SportsApi.api/Controllers/Tournaments/Tournaments/PutUpdateTournament.cs
using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Commands.PutUpdateTournament;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[ApiController]
[Authorize]
[DynamicPermission]
public class PutUpdateTournament(IMediator mediator) : EndpointBaseAsync
    .WithRequest<UpdateTournamentCommand>
    .WithActionResult<UpdateTournamentCommandResult>
{
    [HttpPut("api/v1/tournaments/{id}")]
    [SwaggerOperation(Tags = ["Tournaments"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(UpdateTournamentCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<UpdateTournamentCommandResult>> HandleAsync(
        [FromBody] UpdateTournamentCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator
            .SendCommandAsync<UpdateTournamentCommand, UpdateTournamentCommandResult>(
                request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}
```

### Binding `Id` from both route and body

When the client sends `PUT /api/v1/tournaments/{id}`, the `{id}` segment is automatically model-bound  
into `UpdateTournamentCommand.Id` because the property name matches the route token.  
No extra `[FromRoute]` + `[FromBody]` split is needed as long as the field names align.

If you ever need to split them explicitly:

```csharp
// Controller action signature — explicit split
public override async Task<ActionResult<UpdateTournamentCommandResult>> HandleAsync(
    [FromRoute] Guid id,
    [FromBody]  UpdateTournamentBody body,
    CancellationToken cancellationToken = default)
{
    var command = new UpdateTournamentCommand { Id = id, ... };
    ...
}
```

---

## Checklist

- [ ] Reused the existing `{Entity}ByIdFilter` (not duplicated)
- [ ] Command implements `ICommand<TResult>`; nullable body fields for partial updates
- [ ] Result implements `ICommandResult`
- [ ] Handler follows: fetch → guard → patch → audit → save
- [ ] Handler sets `UpdatedAt` and `UpdatedBy`
- [ ] Handler returns `Result.Fail<T>(message, errorKey)` on errors
- [ ] Controller uses `[ApiController]`, `[Authorize]`, `[DynamicPermission]`
- [ ] Route uses `{id}` token matching `Command.Id` property name
- [ ] `[SwaggerOperation(Tags)]` set
- [ ] `[ProducesResponseType]` for `200` and `404` added
- [ ] `DynamicPermission` will map PUT → `"Update"` automatically

