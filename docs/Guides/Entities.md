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

### `Team`
A sports team that participates in a tournament.

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Team name |
| `LogoUrl` | `string?` | Optional logo URL |
| `TournamentId` | `Guid` | FK to `Tournament` |
| `Tournament` | `Tournament` | Navigation property |
| `Rosters` | `ICollection<Roster>` | Player rosters for this team |
| `Matches` | `ICollection<Match>` | Matches involving this team |

---

### `Roster`
Represents a player entry on a team's roster. *(Model is planned — currently an empty scaffold.)*

---

### `Match`
Represents a game between two teams. *(Model is planned — currently an empty scaffold.)*

---

### `Event`
Represents an in-game event (goal, card, etc.) tied to a match. *(Model is planned — currently an empty scaffold.)*

---

## Enums

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

### `TournamentConfiguration`
- Maps to table `Tournaments`.
- `Name`: required, max 200 chars.
- `Description`: max 2000 chars.
- One-to-many with `Team` (`OnDelete: Restrict`).

