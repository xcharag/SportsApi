# SportsApi — Project Context

## What Is This?

SportsApi is a **.NET 9 REST API** that manages the full lifecycle of sports competitions: tournaments → teams → players → matches → events. It is consumed by a frontend application and relies on an external **SISAPI auth microservice** for JWT authentication.

---

## Domain Model

```
Tournament
 └── TeamParticipation  (team registered IN a specific tournament)
      ├── RoundsClassified   (tracks which round the team is active in)
      ├── Roster             (player enrolled for this team in this tournament)
      │    └── Player        (global player catalogue)
      ├── HomeMatches        (matches where this TP is the home side)
      └── AwayMatches        (matches where this TP is the away side)

Match
 ├── HomeTeam  → TeamParticipation
 ├── AwayTeam  → TeamParticipation
 └── Events
      └── Roster  (who performed the event)

Team   ← global team catalogue, reusable across tournaments
Player ← global player catalogue, reusable across tournaments & teams
```

### Why TeamParticipation?
A `Team` exists globally (e.g. "FC Barcelona"). When it enters a tournament it becomes a `TeamParticipation`, which can have a different display `Name`, `LogoUrl`, and its own roster of players for that tournament season.

### Why Roster?
A `Player` can play for different teams in different tournaments (transfers, loans). A `Roster` entry ties a `Player` to a `TeamParticipation`, recording the shirt number and name for that specific tournament.

### Why RoundsClassified?
`RoundsClassified` is the source of truth for **which teams are still active** in a competition at a given round:
- `Active = true` → team is still competing.
- `Active = false` → team has been eliminated (soft-deleted).
- `Round` / `RoundKey` identify the bracket position (e.g. Group "A", R16 match "AA").
- `GroupPosition` can be used later to seed automatic match generation.

---

## Enums

| Enum | Values |
|---|---|
| `MatchRound` | `Group=1`, `R16=2`, `QuarterFinals=3`, `SemiFinals=4`, `Final=5` |
| `MatchStatus` | `Pending=0`, `InGame=1`, `Finished=2`, `Cancelled=3` |
| `EventType` | `Goal=0`, `YellowCard=1`, `RedCard=2`, `Penalty=3`, `Offside=4`, `Corner=5`, `FreeKick=6` |
| `FavorableTo` | (see domain enum — indicates which side benefits from the event) |

---

## API Surface Summary

| Method | Route | Purpose |
|---|---|---|
| GET | `/api/v1/tournaments` | List tournaments (paginated) |
| GET | `/api/v1/tournaments/{id}` | Get one tournament |
| POST | `/api/v1/tournaments` | Create tournament |
| PUT | `/api/v1/tournaments` | Update tournament |
| DELETE | `/api/v1/tournaments/{id}` | Soft/hard delete tournament |
| **POST** | **`/api/v1/tournaments/{tournamentId}/team-participations`** | **Batch-register teams into a tournament** |
| GET | `/api/v1/team-participations` | List participations |
| GET | `/api/v1/team-participations/{id}` | Get one participation |
| PUT | `/api/v1/team-participations` | Update participation |
| DELETE | `/api/v1/team-participations/{id}` | Delete participation |
| GET | `/api/v1/rounds-classified` | List rounds-classified entries |
| GET | `/api/v1/rounds-classified/{id}` | Get one entry |
| GET | `/api/v1/teams` | List teams |
| GET | `/api/v1/teams/{id}` | Get one team |
| POST | `/api/v1/teams` | Create team |
| PUT | `/api/v1/teams` | Update team |
| DELETE | `/api/v1/teams/{id}` | Delete team |
| GET | `/api/v1/players` | List players |
| GET | `/api/v1/players/{id}` | Get one player |
| POST | `/api/v1/players` | Create player |
| PUT | `/api/v1/players` | Update player |
| DELETE | `/api/v1/players/{id}` | Delete player |
| **POST** | **`/api/v1/team-participations/{teamParticipationId}/rosters`** | **Batch-enroll players into a TeamParticipation** |
| GET | `/api/v1/rosters` | List roster entries |
| GET | `/api/v1/rosters/{id}` | Get one roster entry |
| PUT | `/api/v1/rosters` | Update roster entry |
| DELETE | `/api/v1/rosters/{id}` | Delete roster entry |
| GET | `/api/v1/matches` | List matches |
| GET | `/api/v1/matches/{id}` | Get one match |
| POST | `/api/v1/matches` | Create match |
| PUT | `/api/v1/matches` | Update match (scores, status, round…) |
| DELETE | `/api/v1/matches/{id}` | Delete match |
| GET | `/api/v1/events` | List match events |
| GET | `/api/v1/events/{id}` | Get one event |
| POST | `/api/v1/events` | Record an event |
| PUT | `/api/v1/events` | Update an event |
| DELETE | `/api/v1/events/{id}` | Delete an event |

---

## Architecture

```
SportsApi.api            → Ardalis ApiEndpoints, Swagger, Auth middleware
SportsApi.application    → CQRS handlers (Commands + Queries), auto-registered via reflection
SportsApi.domain         → Entities, abstractions (IRepository, IMediator, ICurrentUser), enums
SportsApi.infrastructure → EF Core (PostgreSQL), JWT, DynamicPermissions, Mediator impl
```

---

## Auth

- JWT Bearer tokens issued by **SISAPI** auth microservice.
- All endpoints require `[Authorize]`.
- Dynamic permission checks are applied globally via `DynamicPermissionGlobalFilter`.
- Token can be sent as `Authorization: Bearer <token>` header **or** `accessToken` cookie.

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| TeamParticipation created via batch endpoint | Frontend "Register Teams" button maps directly to this |
| Roster created via batch endpoint | Frontend "Enroll Players" screen maps to this |
| RoundsClassified uses soft-delete for elimination | `Active=false` = eliminated; enables "still competing?" queries |
| All entities use `Guid` PKs assigned client-side | Allows batching inserts with FK references in a single `SaveChangesAsync` |
| Soft-delete as default | `[Authorize] + ?hardDelete=true` required for physical deletion |

