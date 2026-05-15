# Controllers (API Endpoints)

The API layer (`SportsApi.api`) uses the **Ardalis ApiEndpoints** library. Each HTTP operation is an independent class — there are no traditional controllers with multiple action methods.

---

## Package

```xml
<PackageReference Include="Ardalis.ApiEndpoints" Version="4.1.0" />
```

---

## Endpoint Pattern

Every endpoint inherits from one of the `EndpointBaseAsync` variants:

```csharp
// Command endpoint (POST / PUT / DELETE)
public class PostCreateTournament(IMediator mediator) : EndpointBaseAsync
    .WithRequest<CreateTournamentCommand>
    .WithActionResult<CreateTournamentCommandResult>
{
    [HttpPost("api/v1/tournaments")]
    [SwaggerOperation(Tags = new[] { "Tournaments" })]
    [ProducesResponseType((int)HttpStatusCode.Created, Type = typeof(CreateTournamentCommandResult))]
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
                title: "Failed to create tournament",
                detail: result.Error,
                statusCode: (int)HttpStatusCode.BadRequest);

        return CreatedAtRoute(
            routeName: "GetTournamentById",
            routeValues: new { id = result.Value.Id },
            value: result.Value);
    }
}
```

### Batch endpoints with a parent route parameter

When a batch command creates child resources under a parent (e.g. enrolling players into a `TeamParticipation`), the parent ID comes from the route — **not** from the body. The command property is decorated with `[JsonIgnore]` so Swagger only shows it as a path parameter, and the controller injects it before dispatching:

```csharp
// In the command:
[JsonIgnore]
public Guid TeamParticipationId { get; set; }   // set by controller from route

// In the controller:
[HttpPost("api/v1/team-participations/{teamParticipationId}/rosters")]
public override async Task<ActionResult<...>> HandleAsync(
    [FromBody] EnrollPlayersCommand request,
    CancellationToken cancellationToken = default)
{
    if (RouteData.Values.TryGetValue("teamParticipationId", out var routeId)
        && Guid.TryParse(routeId?.ToString(), out var id))
    {
        request.TeamParticipationId = id;
    }
    // ...
}
```

---

## Folder Convention

```
Controllers/
  Tournaments/
    Tournaments/          ← Tournament CRUD
    TeamParticipations/   ← TeamParticipation CRUD + batch RegisterTeams
    RoundsClassified/     ← RoundsClassified read-only queries
  Teams/
    Teams/                ← Team CRUD
    Players/              ← Player CRUD
    Rosters/              ← Roster CRUD + batch EnrollPlayers
  Matches/
    Matches/              ← Match CRUD
    Events/               ← Event CRUD
```

---

## Authorization

### `[Authorize]`
All endpoints must be decorated with `[Authorize]` to require a valid JWT.

### `[DynamicPermission]` (global filter)

`DynamicPermissionGlobalFilter` is registered globally and runs on every request that is not `[AllowAnonymous]`. It:

1. Extracts the Bearer token from the `Authorization` header or the `accessToken` cookie.
2. Resolves the **controller name** from the `[SwaggerOperation(Tags = ...)]` attribute first, then falls back to the route `controller` segment or the URL path.
3. Maps the HTTP method to an action string:

   | HTTP Method | Action |
   |---|---|
   | GET | `Read` |
   | POST | `Write` |
   | PUT / PATCH | `Update` |
   | DELETE | `Delete` |

4. Calls `IAuthMicroserviceClient.VerifyPermissionAsync(token, module, controller, action)` against the external **SISAPI** auth microservice.
5. Returns `403 Forbidden` if the user lacks permission.

The module name is configured via `appsettings.json`:
```json
"DynamicPermission": {
  "Module": "SportsApi"
}
```

### `[SkipDynamicPermission]` / `[SkipDynamicAuthorization]`

Apply to an endpoint class or action to bypass the dynamic permission check. The JWT is still required unless `[AllowAnonymous]` is also present.

```csharp
[Authorize]
[SkipDynamicPermission]
public class GetPublicInfo(...) : EndpointBaseAsync...
```

### `[RequirePermission]`

For fine-grained, static permission checks on individual endpoints (in addition to or instead of the dynamic global filter):

```csharp
[RequirePermission("SportsApi", "Tournaments", "Write")]
public class PostCreateTournament(...) : ...
```

---

## Swagger / OpenAPI

- **`[SwaggerOperation(Tags = new[] { "GroupName" })]`** groups the endpoint under a named tag in Swagger UI.  
  The `DynamicPermissionGlobalFilter` also reads this tag as the *controller name* for permission checks — **always set it**.
- `[ProducesResponseType]` documents all possible response codes.
- Swagger is available at `/swagger` in development (configured in `Program.cs`).

---

## Response Conventions

| Scenario | HTTP Status | Body |
|---|---|---|
| Created resource | `201 Created` with `Location` header | The created resource |
| Successful query | `200 OK` | Query result / pagination envelope |
| Business rule failure | `400 Bad Request` via `Problem(...)` | `{ title, detail, status }` |
| Unauthenticated | `401 Unauthorized` | JSON error from `GlobalExceptionMiddleware` |
| Permission denied | `403 Forbidden` | Empty body |
| Conflict (duplicate) | `409 Conflict` | JSON error from `GlobalExceptionMiddleware` |
| Unexpected server error | `500 Internal Server Error` | JSON error from `GlobalExceptionMiddleware` |

---

## All Endpoints

### Tournaments

| Method | Route | Endpoint Class | Swagger Tag |
|---|---|---|---|
| `GET` | `/api/v1/tournaments` | `GetAllTournaments` | Tournaments |
| `GET` | `/api/v1/tournaments/{id}` | `GetTournamentById` | Tournaments |
| `POST` | `/api/v1/tournaments` | `PostCreateTournament` | Tournaments |
| `PUT` | `/api/v1/tournaments` | `PutUpdateTournament` | Tournaments |
| `DELETE` | `/api/v1/tournaments/{id}` | `DeleteTournament` | Tournaments |
| `GET` | `/api/v1/team-participations` | `GetAllTeamParticipations` | TeamParticipations |
| `GET` | `/api/v1/team-participations/{id}` | `GetTeamParticipationById` | TeamParticipations |
| `POST` | `/api/v1/team-participations` | `PostCreateTeamParticipation` | TeamParticipations |
| `POST` | `/api/v1/tournaments/{tournamentId}/team-participations` | `PostTeamParticipations` *(batch)* | TeamParticipations |
| `PUT` | `/api/v1/team-participations` | `PutUpdateTeamParticipation` | TeamParticipations |
| `DELETE` | `/api/v1/team-participations/{id}` | `DeleteTeamParticipation` | TeamParticipations |
| `GET` | `/api/v1/rounds-classified` | `GetAllRoundsClassified` | RoundsClassified |
| `GET` | `/api/v1/rounds-classified/{id}` | `GetRoundsClassifiedById` | RoundsClassified |

### Teams

| Method | Route | Endpoint Class | Swagger Tag |
|---|---|---|---|
| `GET` | `/api/v1/teams` | `GetAllTeams` | Teams |
| `GET` | `/api/v1/teams/{id}` | `GetTeamById` | Teams |
| `POST` | `/api/v1/teams` | `PostCreateTeam` | Teams |
| `PUT` | `/api/v1/teams` | `PutUpdateTeam` | Teams |
| `DELETE` | `/api/v1/teams/{id}` | `DeleteTeam` | Teams |
| `GET` | `/api/v1/players` | `GetAllPlayers` | Players |
| `GET` | `/api/v1/players/{id}` | `GetPlayerById` | Players |
| `POST` | `/api/v1/players` | `PostCreatePlayer` | Players |
| `PUT` | `/api/v1/players` | `PutUpdatePlayer` | Players |
| `DELETE` | `/api/v1/players/{id}` | `DeletePlayer` | Players |
| `GET` | `/api/v1/rosters` | `GetAllRosters` | Rosters |
| `GET` | `/api/v1/rosters/{id}` | `GetRosterById` | Rosters |
| `POST` | `/api/v1/rosters` | `PostCreateRoster` | Rosters |
| `POST` | `/api/v1/team-participations/{teamParticipationId}/rosters` | `PostRosters` *(batch)* | Rosters |
| `PUT` | `/api/v1/rosters` | `PutUpdateRoster` | Rosters |
| `DELETE` | `/api/v1/rosters/{id}` | `DeleteRoster` | Rosters |

### Matches

| Method | Route | Endpoint Class | Swagger Tag |
|---|---|---|---|
| `GET` | `/api/v1/matches` | `GetAllMatches` | Matches |
| `GET` | `/api/v1/matches/{id}` | `GetMatchById` | Matches |
| `POST` | `/api/v1/matches` | `PostCreateMatch` | Matches |
| `PUT` | `/api/v1/matches` | `PutUpdateMatch` | Matches |
| `DELETE` | `/api/v1/matches/{id}` | `DeleteMatch` | Matches |
| `GET` | `/api/v1/events` | `GetAllEvents` | Events |
| `GET` | `/api/v1/events/{id}` | `GetEventById` | Events |
| `POST` | `/api/v1/events` | `PostCreateEvent` | Events |
| `PUT` | `/api/v1/events` | `PutUpdateEvent` | Events |
| `DELETE` | `/api/v1/events/{id}` | `DeleteEvent` | Events |
