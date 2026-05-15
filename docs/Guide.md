# SportsApi — Developer Guide

SportsApi is a **.NET 9** Web API for managing sports tournaments, teams, matches, and events. It follows **Clean Architecture** with CQRS and the Repository / Unit-of-Work patterns backed by **PostgreSQL**.

---

## Quick Start

### Prerequisites
- .NET 9 SDK
- PostgreSQL server (local or remote)

### 1. Configure the connection string

Edit `SportsApi.api/appsettings.json` (or `appsettings.Development.json` for local dev):

```json
"ConnectionStrings": {
  "CoreConnection": "Host=localhost;Port=5432;Database=SportsApiDb;Username=postgres;Password=your_password"
}
```

### 2. Configure JWT

```json
"JwtSettings": {
  "SecretKey": "<same_secret_as_auth_microservice>",
  "Issuer": "SisApi",
  "Audience": "SisApiClient"
}
```

### 3. Apply migrations

```powershell
dotnet ef migrations add InitialCreate `
  --project SportsApi.infrastructure `
  --startup-project SportsApi.api

dotnet ef database update `
  --project SportsApi.infrastructure `
  --startup-project SportsApi.api
```

### 4. Run

```powershell
dotnet run --project SportsApi.api
```

Swagger UI is available at `https://localhost:<port>/swagger` in development.

---

## Guide Index

### Architecture & Concepts

| Guide | Description |
|---|---|
| [Structure](./Guides/Structure.md) | Solution layout, project responsibilities, and folder conventions |
| [Entities](./Guides/Entities.md) | Domain entities, `BaseEntity`, enums, exceptions, and EF Core configuration |
| [Patterns](./Guides/Patterns.md) | Result pattern, CQRS/Mediator, Repository, Unit of Work, Specifications, soft-delete, pagination |
| [Application](./Guides/Application.md) | How to add Commands and Queries; handler registration; existing operations |
| [Controllers](./Guides/Controllers.md) | Ardalis ApiEndpoints pattern, authorization, Swagger, response conventions |
| [Infrastructure](./Guides/Infrastructure.md) | EF Core/PostgreSQL, JWT auth, `ICurrentUser`, dynamic permissions, exception middleware |

### Step-by-step: Adding new endpoints

| Guide | HTTP verb | Description |
|---|---|---|
| [New GET endpoint](./GuideNewImplementations/NewGetController.md) | `GET` | Paginated list with filters + single record by ID |
| [New POST endpoint](./GuideNewImplementations/NewPostController.md) | `POST` | Create a new record, return 201 + Location header |
| [New PUT endpoint](./GuideNewImplementations/NewPutController.md) | `PUT` | Partial update of an existing record |
| [New DELETE endpoint](./GuideNewImplementations/NewDeleteController.md) | `DELETE` | Soft-delete (default) or hard-delete with `HardDelete` flag |

### Frontend integration guides (module-by-module)

| Guide | Description |
|---|---|
| [Tournaments](./Integration/Tournaments.md) | Tournament CRUD, batch team registration, standings, bracket, top scorers |
| [Teams](./Integration/Teams.md) | Team catalogue CRUD, team profile endpoint |
| [Players](./Integration/Players.md) | Player catalogue CRUD, batch enrollment, stats + profile endpoints |
| [Matches](./Integration/Matches.md) | Match CRUD, auto-score on goal, auto-advance on finish, SSE live stream |
| [Events](./Integration/Events.md) | Match event recording, auto-score update side-effect |

---

## Architecture Overview

```
                  ┌──────────────────────────────────────┐
                  │           SportsApi.api               │
                  │  Ardalis Endpoints · Swagger · Auth   │
                  └──────────────────┬───────────────────┘
                                     │ IMediator
                  ┌──────────────────▼───────────────────┐
                  │        SportsApi.application          │
                  │   Commands · Queries · Handlers       │
                  └──────────────────┬───────────────────┘
                                     │ IRepository<T>
                                     │ ICoreUnitOfWork
                                     │ ICurrentUser
                  ┌──────────────────▼───────────────────┐
                  │          SportsApi.domain             │
                  │  Entities · Abstractions · Enums      │
                  └──────────────────▲───────────────────┘
                                     │ implements
                  ┌──────────────────┴───────────────────┐
                  │       SportsApi.infrastructure        │
                  │  EF Core · PostgreSQL · JWT · Auth    │
                  └──────────────────────────────────────┘
```

---

## Key Libraries

| Library | Version | Purpose |
|---|---|---|
| `Ardalis.ApiEndpoints` | 4.1.0 | One-class-per-endpoint pattern |
| `Ardalis.Specification` | 9.3.1 | Query specification pattern |
| `Ardalis.Specification.EntityFrameworkCore` | 9.3.1 | EF Core integration for specifications |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.0.4 | PostgreSQL EF Core provider |
| `Microsoft.EntityFrameworkCore` | 9.0.15 | ORM |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 9.0.15 | JWT validation |
| `Swashbuckle.AspNetCore` | 9.0.6 | Swagger/OpenAPI |
| `Swashbuckle.AspNetCore.Annotations` | 9.0.6 | `[SwaggerOperation]` and friends |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | 9.0.15 | DI in Application layer |

---

## Adding a New Feature — Checklist

1. **Domain** — add or extend entity in `SportsApi.domain/Modules/<Module>/`.
2. **EF Configuration** — add `<Entity>Configuration : BaseEntityConfiguration<T>` in `SportsApi.infrastructure/Persistence/Core/EntityFramework/Configurations/`.
3. **Migration** — run `dotnet ef migrations add ...`.
4. **Application** — create `Command` + `CommandHandler` + `CommandResult` (or `Query` equivalents) in `SportsApi.application/Modules/<Module>/<Aggregate>/Commands/<VerbActionName>/`.
5. **Endpoint** — create one file in `SportsApi.api/Controllers/<Module>/<Aggregate>/` inheriting `EndpointBaseAsync`.
6. **No manual DI registration** — handlers are auto-discovered; repositories and the Unit of Work are generic and already registered.

> **Exception — SSE endpoints**: Use plain `ControllerBase` (not Ardalis) when you need direct access to `HttpContext.Response.Body` for streaming. See `GetMatchLive.cs` for the reference implementation.

---

## Live Scores (SSE)

The API supports real-time match updates via **Server-Sent Events**:

```
GET /api/v1/matches/{matchId}/live
```

- Returns `Content-Type: text/event-stream`.
- Sends a `heartbeat` event on connect.
- Streams `update` events as JSON whenever the score or match status changes.
- Because `EventSource` in browsers cannot set custom headers, the JWT is accepted via the `?access_token=<token>` query parameter.
- The `IMatchLiveHub` singleton (`SportsApi.infrastructure.Services.Live.MatchLiveHub`) manages channels per match using `ConcurrentDictionary<Guid, ConcurrentBag<Channel<string>>>`.
- Handlers that publish updates inject `SportsApi.domain.Abstractions.Live.IMatchLiveHub` (the thin domain interface with just `Publish`).
- The SSE controller injects `SportsApi.infrastructure.Services.Live.IMatchLiveHub` (which also exposes `Subscribe` / `Unsubscribe`).

### Event payload shapes

**Score update** (emitted when a `Goal` event is created):
```json
{ "type": "score", "matchId": "uuid", "homeScore": 2, "awayScore": 1, "eventId": "uuid", "eventType": 0, "minute": 34, "favorableTo": 0 }
```

**Status update** (emitted when match status changes):
```json
{ "type": "status", "matchId": "uuid", "status": "Finished", "homeScore": 2, "awayScore": 1 }
```

**Generic event** (emitted for non-goal events):
```json
{ "type": "event", "matchId": "uuid", "eventId": "uuid", "eventType": 1, "minute": 55, "favorableTo": 1 }
```

