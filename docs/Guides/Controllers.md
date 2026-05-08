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

---

## Folder Convention

Endpoints are organized under `Controllers/` mirroring the domain module hierarchy. Each aggregate gets its own sub-folder:

```
Controllers/
  Tournaments/
    Tournaments/      ← aggregate: Tournament
      PostCreateTournament.cs
      GetTournamentById.cs     (example)
      GetTournaments.cs        (example)
  Matches/
    Matches/          ← aggregate: Match
    Events/           ← aggregate: Event
  Teams/
    Teams/            ← aggregate: Team
    Players/          ← aggregate: Player (Roster)
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

## Existing Endpoints

| Method | Route | Endpoint Class | Module |
|---|---|---|---|
| `POST` | `/api/v1/tournaments` | `PostCreateTournament` | Tournaments |

