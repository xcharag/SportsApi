# Guide — Creating a POST Endpoint

This guide shows you how to add a **create** endpoint to the API.

| HTTP | Route pattern | Response |
|---|---|---|
| `POST` | `/api/v1/{resource}` | `201 Created` with location header |

The Tournaments module (`PostCreateTournament`) is used as the working reference.

---

## Folder layout

```
SportsApi.application/Modules/{Domain}/{Entity}/
└── Commands/
    └── Post{Create}{Entity}/
        ├── Create{Entity}Command.cs
        ├── Create{Entity}CommandHandler.cs
        └── Create{Entity}CommandResult.cs

SportsApi.api/Controllers/{Domain}/{Entity}/
└── Post{Create}{Entity}.cs
```

---

## Step 1 — Command class

Implement `ICommand<TResult>`. Properties mirror the request body fields that the client sends.

```csharp
// Commands/PostCreateTournament/CreateTournamentCommand.cs
using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.PostCreateTournament;

public sealed record CreateTournamentCommand : ICommand<CreateTournamentCommandResult>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
}
```

> Use `sealed record` for commands — records give you structural equality for free, `sealed` prevents unintended inheritance.

---

## Step 2 — Result class

Implement `ICommandResult`. Return the minimum needed to let the client navigate to the new resource.

```csharp
// Commands/PostCreateTournament/CreateTournamentCommandResult.cs
using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.PostCreateTournament;

public class CreateTournamentCommandResult : ICommandResult
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

---

## Step 3 — Command Handler

Implement `ICommandHandler<TCommand, TResult>`.  
Inject `IRepository<TEntity>`, `ICoreUnitOfWork`, and `ICurrentUser` (for audit fields).

```csharp
// Commands/PostCreateTournament/CreateTournamentCommandHandler.cs
using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.PostCreateTournament;

public class CreateTournamentCommandHandler(
    IRepository<Tournament> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<CreateTournamentCommand, CreateTournamentCommandResult>
{
    public async Task<Result<CreateTournamentCommandResult>> HandleAsync(
        CreateTournamentCommand command,
        CancellationToken cancellationToken)
    {
        var username = currentUser.Username;

        var entity = new Tournament
        {
            Name        = command.Name,
            Description = command.Description,
            StartDate   = command.StartDate,
            EndDate     = command.EndDate,
            LogoUrl     = command.LogoUrl,
            BannerUrl   = command.BannerUrl,
            CreatedBy   = username          // BaseEntity audit field
        };

        var saveResult = await repository.SaveAsync(entity, cancellationToken);
        if (saveResult.IsFailure)
            return Result.Fail<CreateTournamentCommandResult>(saveResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateTournamentCommandResult
        {
            Id   = entity.Id,
            Name = entity.Name
        });
    }
}
```

### Rules

| Concern | Rule |
|---|---|
| Audit | Set `entity.CreatedBy = currentUser.Username` before saving |
| Persist | Call `repository.SaveAsync`, then `unitOfWork.SaveChangesAsync` |
| Error propagation | Return `Result.Fail<T>(...)` — never throw inside a handler |
| Unique violations | Caught by `EfUnitOfWork` → becomes `DomainConflictException` with `ErrorKey` |

---

## Step 4 — API Controller

```csharp
// SportsApi.api/Controllers/Tournaments/Tournaments/PostCreateTournament.cs
using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Commands.PostCreateTournament;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[ApiController]
[Authorize]
[DynamicPermission]
public class PostCreateTournament(IMediator mediator) : EndpointBaseAsync
    .WithRequest<CreateTournamentCommand>
    .WithActionResult<CreateTournamentCommandResult>
{
    [HttpPost("api/v1/tournaments")]
    [SwaggerOperation(Tags = ["Tournaments"])]
    [ProducesResponseType((int)HttpStatusCode.Created,    Type = typeof(CreateTournamentCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationProblemDetails))]
    public override async Task<ActionResult<CreateTournamentCommandResult>> HandleAsync(
        [FromBody] CreateTournamentCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator
            .SendCommandAsync<CreateTournamentCommand, CreateTournamentCommandResult>(
                request, cancellationToken);

        if (result.IsFailure)
            return Problem(
                title:      "Failed to create tournament",
                detail:     result.Error,
                statusCode: (int)HttpStatusCode.BadRequest);

        return CreatedAtRoute(
            routeName:   "GetTournamentById",          // ← Name on the GET-by-ID route
            routeValues: new { id = result.Value.Id },
            value:       result.Value);
    }
}
```

### Key points

| Point | Detail |
|---|---|
| `[FromBody]` | POST body is always `[FromBody]` |
| `CreatedAtRoute` | Generates the `Location` header pointing to the new resource; the route name must match the GET-by-ID endpoint's `Name` |
| `[SwaggerOperation(Tags)]` | Must match the resource name registered in SISAPI permissions (e.g. `"Tournaments"`) |
| `DynamicPermission` maps POST → `"Write"` action in the permission check |

---

## Checklist

- [ ] Command implements `ICommand<TResult>` using `sealed record`
- [ ] Result implements `ICommandResult` and includes at least `Id`
- [ ] Handler sets `CreatedBy` from `ICurrentUser.Username`
- [ ] Handler calls `SaveAsync` → `SaveChangesAsync` in that order
- [ ] Handler returns `Result.Fail<T>(message, errorKey)` on errors (never throws)
- [ ] Controller uses `[ApiController]`, `[Authorize]`, `[DynamicPermission]`
- [ ] Controller binds request with `[FromBody]`
- [ ] Controller returns `CreatedAtRoute(...)` on success
- [ ] `[SwaggerOperation(Tags)]` set
- [ ] `[ProducesResponseType]` for `201` and `400` added

