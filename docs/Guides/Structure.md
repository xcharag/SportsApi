# Solution Structure

SportsApi is a **.NET 9** Web API built with Clean Architecture principles. The solution is divided into four projects:

```
SportsApi.sln
├── SportsApi.api               # Entry point – HTTP endpoints & middleware wiring
├── SportsApi.application       # Use-case logic – commands, queries, handlers
├── SportsApi.domain            # Core model – entities, abstractions, enums
└── SportsApi.infrastructure    # External concerns – EF Core, auth, messaging
```

---

## Dependency Graph

```
SportsApi.api
  └── SportsApi.application
        └── SportsApi.domain
  └── SportsApi.infrastructure
        └── SportsApi.application
        └── SportsApi.domain
```

`SportsApi.domain` has **no project dependencies** — only `Ardalis.Specification`. Everything else depends inward toward it.

---

## Project Responsibilities

### `SportsApi.domain`
- Defines all domain **entities** (`Tournament`, `Team`, `Roster`, `Match`, `Event`).
- Contains all **abstractions** (interfaces) that the other layers depend on:
  - `IMediator`, `ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler`
  - `IRepository<T>`, `ICoreUnitOfWork`, `IBaseRepository<T>`
  - `ICurrentUser`, `IAuthMicroserviceClient`
  - `Result<T>`, `PaginationResult<T>`, `PaginationSpecification<T>`
- Defines **enums**: `MatchStatus`, `EventType`, `FavorableTo`, `Module`.
- Contains **exceptions**: `DomainConflictException`.
- Has **zero infrastructure dependencies**.

### `SportsApi.application`
- Implements business use cases as **Commands** and **Queries** (CQRS).
- Each module folder mirrors the domain module: `Modules/Tournaments/Tournaments/Commands/...`
- `ApplicationServiceExtensions.AddApplication()` auto-registers all handlers via reflection.
- Depends only on `SportsApi.domain`.

### `SportsApi.infrastructure`
- Provides concrete implementations for every domain abstraction:
  - **EF Core / PostgreSQL**: `CoreDbContext`, `EfRepository<T>`, `EfUnitOfWork`
  - **Messaging**: `Mediator` (custom, service-locator-based)
  - **Auth**: `CurrentUserService`, `AuthMicroserviceClient`, `DynamicPermissionGlobalFilter`, `PermissionPolicyProvider`
  - **Middleware**: `GlobalExceptionMiddleware`
- `InfrastructureServiceExtensions.AddInfrastructure()` wires everything up.

### `SportsApi.api`
- ASP.NET Core Web API host.
- Endpoints use the **Ardalis ApiEndpoints** pattern (one class per endpoint).
- Registers Swagger/OpenAPI, authentication, and authorization middleware.
- Controllers are organized by domain module inside `Controllers/`.

---

## Folder Layout (inside each project)

### `SportsApi.domain`
```
Abstractions/
  Auth/           ICurrentUser, IAuthMicroserviceClient
  Dtos/           Result<T>, PaginationResult<T>
  Entities/       Entity (base), BaseEntity
  Exceptions/     DomainConflictException
  Messaging/
    Commands/     ICommand, ICommandHandler
    Queries/      IQuery, IQueryHandler
    IMediator
  Persistence/    IRepository<T>, ICoreUnitOfWork, IBaseRepository<T>
  Specifications/ PaginationSpecification<T>
Enums/
  Status/         MatchStatus
  Types/          EventType
  FavorableTo, Module
Modules/
  Matches/        Match, Event
  Teams/          Team, Roster
  Tournaments/    Tournament
```

### `SportsApi.application`
```
Modules/
  Tournaments/
    Tournaments/
      Commands/
        PostCreateTournament/   CreateTournamentCommand, Handler, Result
  Matches/    (planned)
  Teams/      (planned)
ApplicationServiceExtensions.cs
```

### `SportsApi.infrastructure`
```
Persistence/Core/EntityFramework/
  Configurations/   BaseEntityConfiguration<T>, TournamentConfiguration
  Repositories/     EfRepository<T>
  CoreDbContext.cs
  EfUnitOfWork.cs
Services/
  Auth/       CurrentUserService, AuthMicroserviceClient, DynamicPermissionGlobalFilter,
              PermissionPolicyProvider, PermissionAuthorizationHandler,
              RequirePermissionAttribute, SkipDynamicPermissionAttribute
  Messaging/  Mediator (MediatorMessaging.cs)
  Middleware/ GlobalExceptionMiddleware
InfrastructureServiceExtension.cs
```

### `SportsApi.api`
```
Controllers/
  Tournaments/Tournaments/  PostCreateTournament
  Matches/Matches/          (planned)
  Matches/Events/           (planned)
  Teams/Teams/              (planned)
  Teams/Players/            (planned)
Program.cs
appsettings.json
```

