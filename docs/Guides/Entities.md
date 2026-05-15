# Domain Entities

All domain entities live in `SportsApi.domain/Modules/` and inherit from `BaseEntity`.

---

## Base Classes

### `Entity` (abstract)
The root base class in `SportsApi.domain.Abstractions.Entities`. It is intentionally empty — it acts as a marker so that `IBaseRepository<TEntity>` can constrain on `Entity` while `IRepository<TEntity>` further constrains on `BaseEntity`.

### `BaseEntity : Entity`
Every aggregate root or entity used with the repository inherits from `BaseEntity`.

| Property | Type | Description |
|---|---|---|
| `Id` | `Guid` | Primary key, auto-generated |
| `Active` | `bool` | Soft-delete flag (default `true`) |
| `CreatedAt` | `DateTime` | Set at creation time |
| `CreatedBy` | `string` | Username of the creator (required) |
| `UpdatedAt` | `DateTime?` | Set when the record is modified |
| `UpdatedBy` | `string?` | Username of the modifier |
| `DeletedAt` | `DateTime?` | Set on soft-delete |
| `DeletedBy` | `string?` | Username of who deleted it |
| `TournamentIdOwner` | `Guid` | Multi-tenancy owner — every entity belongs to a tournament scope |

> **Note:** `Active = false` records are filtered out globally by EF Core's query filter configured in `BaseEntityConfiguration<T>`. Use `IgnoreQueryFilters()` (exposed via `GetBySpecificationAnyStateAsync`) to query soft-deleted records.

---

## Domain Entities

### `Tournament`
The top-level aggregate. Represents a sports tournament.

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Tournament name (max 200 chars) |
| `Description` | `string` | Tournament description (max 2000 chars) |
| `StartDate` | `DateTime` | Start date |
| `EndDate` | `DateTime` | End date |
| `LogoUrl` | `string?` | Optional logo image URL |
| `BannerUrl` | `string?` | Optional banner image URL |
| `TeamsParticipations` | `ICollection<TeamParticipation>?` | Teams enrolled in this tournament |

Database table: **`Tournaments`**. Configured in `TournamentConfiguration`.

---

### `TeamParticipation`
A `Team` registered in a specific `Tournament`. It carries the display name and logo for that season and owns the squad (Rosters) and match history for that tournament.

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Display name for this tournament season (can differ from global `DefaultName`) |
| `LogoUrl` | `string?` | Optional override logo for this season |
| `TeamId` | `Guid` | FK to `Team` (global catalogue) |
| `Team` | `Team` | Navigation to global team |
| `TournamentId` | `Guid` | FK to `Tournament` |
| `Tournament` | `Tournament` | Navigation to tournament |
| `HomeMatches` | `ICollection<Match>?` | Matches where this TP is the home side |
| `AwayMatches` | `ICollection<Match>?` | Matches where this TP is the away side |
| `Rosters` | `ICollection<Roster>?` | Players enrolled for this team in this tournament |
| `RoundsClassified` | `ICollection<RoundsClassified>?` | Round progression entries |

Database table: **`TeamParticipations`**. Configured in `TeamParticipationConfiguration`.

---

### `RoundsClassified`
Tracks which round a `TeamParticipation` is currently active in. System-managed — created automatically via the batch "Register Teams" endpoint, not directly by users.

| Property | Type | Description |
|---|---|---|
| `Round` | `MatchRound` | Which stage (Group, R16, QF, SF, Final) |
| `RoundKey` | `string` | Bracket slot identifier: `"A"`, `"B"` for groups; `"AA"`, `"AB"` from R16 |
| `GroupPosition` | `int?` | Seed position within the group (reserved for auto-match generation) |
| `TeamParticipationId` | `Guid` | FK to `TeamParticipation` |
| `TeamParticipation` | `TeamParticipation?` | Navigation |

`Active = false` (via soft-delete) means the team has been **eliminated**. `Active = true` means still competing.

Database table: **`RoundsClassified`**. Configured in `RoundsClassifiedConfiguration`.

---

### `Team`
Global team catalogue entry. Reusable across tournaments and seasons.

| Property | Type | Description |
|---|---|---|
| `DefaultName` | `string` | Canonical team name |
| `DefaultLogoUrl` | `string?` | Default logo URL (can be overridden per `TeamParticipation`) |
| `TeamParticipations` | `ICollection<TeamParticipation>?` | All tournament registrations for this team |

Database table: **`Teams`**. Configured in `TeamConfiguration`.

---

### `Player`
Global player catalogue entry. Reusable across teams and tournaments.

| Property | Type | Description |
|---|---|---|
| `FullName` | `string` | Player's full name |
| `Ci` | `string?` | National identity / document number |
| `PhoneNumber` | `string?` | Contact phone number |
| `IsForeigner` | `bool` | Whether the player is a foreign national |
| `Rosters` | `ICollection<Roster>?` | All roster entries across teams and tournaments |

Database table: **`Players`**. Configured in `PlayerConfiguration`.

---

### `Roster`
Ties a `Player` to a `TeamParticipation`. Records the shirt number and name for a specific tournament season. System-managed — created via the batch "Enroll Players" endpoint.

| Property | Type | Description |
|---|---|---|
| `ShirtNumber` | `int?` | Shirt number for this tournament |
| `ShirtName` | `string?` | Name printed on the shirt (may differ from `FullName`) |
| `PlayerId` | `Guid` | FK to `Player` |
| `Player` | `Player` | Navigation |
| `TeamParticipationId` | `Guid` | FK to `TeamParticipation` |
| `Team` | `TeamParticipation` | Navigation (nav property named `Team`) |
| `Events` | `ICollection<Event>?` | Match events performed by this player in this tournament |

Database table: **`Rosters`**. Configured in `RosterConfiguration`.

---

### `Match`
A game between two `TeamParticipation` entries within a tournament round.

| Property | Type | Description |
|---|---|---|
| `MatchDay` | `int` | Round / match-day number |
| `ScoreHomeTeam` | `int` | Goals scored by the home team |
| `ScoreAwayTeam` | `int` | Goals scored by the away team |
| `MatchDate` | `DateTime` | Scheduled date and time |
| `Field` | `string?` | Venue / pitch name |
| `Location` | `string?` | City or address |
| `Status` | `MatchStatus` | Current state (`Pending`, `InGame`, `Finished`, `Cancelled`) |
| `Round` | `MatchRound` | Tournament stage |
| `NewMatchId` | `Guid?` | ID of the next-round match (for bracket progression) |
| `HomeTeamId` | `Guid` | FK to `TeamParticipation` (home side) |
| `HomeTeam` | `TeamParticipation` | Navigation |
| `AwayTeamId` | `Guid` | FK to `TeamParticipation` (away side) |
| `AwayTeam` | `TeamParticipation` | Navigation |
| `Events` | `ICollection<Event>?` | All events that occurred in this match |

Database table: **`Matches`**. Configured in `MatchConfiguration`.

---

### `Event`
An in-game occurrence (goal, card, etc.) tied to a specific match and player roster entry.

| Property | Type | Description |
|---|---|---|
| `Minute` | `int` | Match minute when the event occurred |
| `FavorableTo` | `FavorableTo` | Which side benefits from this event |
| `EventType` | `EventType` | Category of event |
| `RosterId` | `Guid` | FK to `Roster` (player who performed the action) |
| `Roster` | `Roster` | Navigation |
| `MatchId` | `Guid` | FK to `Match` |
| `Match` | `Match` | Navigation |

Database table: **`Events`**. Configured in `EventConfiguration`.

---

## Enums

### `MatchRound` (`SportsApi.domain.Enums`)
| Value | Int | Meaning |
|---|---|---|
| `Group` | 1 | Group stage |
| `R16` | 2 | Round of 16 |
| `QuarterFinals` | 3 | Quarter-finals |
| `SemiFinals` | 4 | Semi-finals |
| `Final` | 5 | Final |

### `MatchStatus` (`SportsApi.domain.Enums.Status`)
| Value | Int | Meaning |
|---|---|---|
| `Pending` | 0 | Match has not started yet |
| `InGame` | 1 | Match is in progress |
| `Finished` | 2 | Match is completed |
| `Cancelled` | 3 | Match was cancelled |

### `EventType` (`SportsApi.domain.Enums.Types`)
| Value | Int | Meaning |
|---|---|---|
| `Goal` | 0 | A goal was scored |
| `YellowCard` | 1 | Yellow card issued |
| `RedCard` | 2 | Red card issued |
| `Penalty` | 3 | Penalty awarded |
| `Offside` | 4 | Offside call |
| `Corner` | 5 | Corner kick |
| `FreeKick` | 6 | Free kick awarded |

### `FavorableTo` (`SportsApi.domain.Enums`)
| Value | Int | Meaning |
|---|---|---|
| `Home` | 0 | Event/outcome favours the home team |
| `Away` | 1 | Event/outcome favours the away team |

### `Module` (`SportsApi.domain.Enums`)
| Value | Int | Meaning |
|---|---|---|
| `SportsApi` | 3 | Permission system module identifier |

---

## Exceptions

### `DomainConflictException`
Thrown when a unique-constraint violation is caught at the database level and translated to a domain error.

```csharp
public sealed class DomainConflictException(string message, string errorKey) : Exception(message)
{
    public string ErrorKey { get; }
}
```

`EfUnitOfWork` catches `PostgresException` with SQLSTATE `23505` and maps it to this exception using a static lookup table of constraint names → human-readable messages.

---

## Entity Configuration (EF Core)

Each entity has a corresponding `IEntityTypeConfiguration<T>` class in `SportsApi.infrastructure/Persistence/Core/EntityFramework/Configurations/`.

### `BaseEntityConfiguration<TEntity>` (abstract)

Automatically applied to every entity. Configures:
- Primary key on `Id`.
- `Active` default value `true`.
- `CreatedAt` / `CreatedBy` as required.
- Global query filter `e => e.Active` (soft-delete).

Subclasses override `ConfigureEntity(EntityTypeBuilder<TEntity> builder)` to add entity-specific configuration.

### Entity Configurations

| Class | Table | Key relationships |
|---|---|---|
| `TournamentConfiguration` | `Tournaments` | One-to-many with `TeamParticipation` (Restrict) |
| `TeamParticipationConfiguration` | `TeamParticipations` | FK to `Team`, FK to `Tournament`; one-to-many with `Roster`, `RoundsClassified`, `Match` |
| `RoundsClassifiedConfiguration` | `RoundsClassified` | FK to `TeamParticipation` (Restrict) |
| `TeamConfiguration` | `Teams` | One-to-many with `TeamParticipation` |
| `PlayerConfiguration` | `Players` | One-to-many with `Roster` |
| `RosterConfiguration` | `Rosters` | FK to `Player`, FK to `TeamParticipation`; one-to-many with `Event` |
| `MatchConfiguration` | `Matches` | FK to `HomeTeam` (TeamParticipation), FK to `AwayTeam` (TeamParticipation) |
| `EventConfiguration` | `Events` | FK to `Roster`, FK to `Match` |
