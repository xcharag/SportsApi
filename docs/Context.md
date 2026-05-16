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
- `Round` / `RoundKey` identify the bracket position (e.g. Group `"A"`, R16 match `"AA"`).
- `NextRoundKey` (string?) — the bracket slot this team would move into when they win (links to the next round's RC entry).
- `GroupPosition` can be used later to seed automatic match generation.

### Key new domain properties
| Entity | Property | Description |
|---|---|---|
| `Tournament` | `TeamsPerGroupThatClassify` (int, default 2) | How many teams per group advance to the knockout stage |
| `RoundsClassified` | `NextRoundKey` (string?) | Bracket slot the winner moves into (used by auto-advance logic) |
| `Match` (Update only) | `ManualWinnerId` (Guid?) | Override winner for penalty shoot-outs; required when knockout match ends in a draw |

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
| **GET** | **`/api/v1/players/{id}/stats`** | **Aggregated stats for all tournaments (goals, cards…)** |
| **GET** | **`/api/v1/players/{id}/profile`** | **Full player profile: teams played, career stats, event log** |
| **POST** | **`/api/v1/team-participations/{teamParticipationId}/rosters`** | **Batch-enroll players into a TeamParticipation** |
| GET | `/api/v1/rosters` | List roster entries |
| GET | `/api/v1/rosters/{id}` | Get one roster entry |
| PUT | `/api/v1/rosters` | Update roster entry |
| DELETE | `/api/v1/rosters/{id}` | Delete roster entry |
| GET | `/api/v1/matches` | List matches (filters: `tournamentId`, `round`, `status`, `teamParticipationId`, **`teamId`** — cross-tournament) |
| GET | `/api/v1/matches/{id}` | Get one match |
| POST | `/api/v1/matches` | Create match |
| PUT | `/api/v1/matches` | Update match (scores, status, round…) |
| DELETE | `/api/v1/matches/{id}` | Delete match |
| GET | `/api/v1/events` | List match events |
| GET | `/api/v1/events/{id}` | Get one event |
| POST | `/api/v1/events` | Record an event |
| PUT | `/api/v1/events` | Update an event |
| DELETE | `/api/v1/events/{id}` | Delete an event |
| **GET** | **`/api/v1/tournaments/{id}/standings`** | **Group stage standings (optional `?groupKey=A`)** |
| **GET** | **`/api/v1/tournaments/{id}/bracket`** | **Playoff bracket (R16 → Final) with slot/match data** |
| **GET** | **`/api/v1/tournaments/{id}/top-scorers`** | **Top scorers for a tournament (`?limit=N`)** |
| **GET** | **`/api/v1/teams/{id}/profile`** | **Team profile: history, all-time top scorers, career stats, W/D/L record** |
| **GET** | **`/api/v1/matches/{id}/live`** | **SSE stream — real-time score + status updates (text/event-stream)** |

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
- Read-only public endpoints are decorated with `[SkipDynamicPermission]` and still validate the JWT (auth is still required).
- Token can be sent as:
  - `Authorization: Bearer <token>` header
  - `accessToken` cookie
  - `?access_token=<token>` query string — **SSE clients only** (browsers cannot set headers on `EventSource`)

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| TeamParticipation created via batch endpoint | Frontend "Register Teams" button maps directly to this |
| Roster created via batch endpoint | Frontend "Enroll Players" screen maps to this |
| RoundsClassified uses soft-delete for elimination | `Active=false` = eliminated; enables "still competing?" queries |
| All entities use `Guid` PKs assigned client-side | Allows batching inserts with FK references in a single `SaveChangesAsync` |
| Soft-delete as default | `[Authorize] + ?hardDelete=true` required for physical deletion |
| Auto-advance on Match finish | When a knockout match status → `Finished`, the winning team's `RoundsClassified` is promoted to the next round using `NextRoundKey`; the loser's is soft-deleted |
| Score auto-update on Goal event | Creating a `Goal` event increments `HomeScore` or `AwayScore` on the parent `Match` automatically |
| SSE for live scores | `GET /api/v1/matches/{id}/live` streams JSON over `text/event-stream`; token passed via `?access_token=` because `EventSource` cannot set headers |

