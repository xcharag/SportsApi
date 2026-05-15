# Application Layer

The application layer (`SportsApi.application`) contains all business use cases modelled as Commands and Queries following the CQRS pattern.

---

## Service Registration

```csharp
// Program.cs / startup
builder.Services.AddApplication();
```

`ApplicationServiceExtensions.AddApplication()` scans the assembly via reflection and auto-registers every `ICommandHandler<>`, `ICommandHandler<,>`, and `IQueryHandler<,>` implementation as a **scoped** service. No manual DI wiring is needed when adding new handlers.

---

## Folder Convention

Each domain module has a matching folder under `Modules/`. Inside each module there are `Commands/` and `Queries/` folders. Each operation gets its own sub-folder containing exactly three files:

```
Modules/
  <DomainModule>/
    <AggregateGroup>/
      Commands/
        <VerbActionName>/
          <VerbActionName>Command.cs        ← input (record / sealed record)
          <VerbActionName>CommandHandler.cs ← logic
          <VerbActionName>CommandResult.cs  ← output
      Queries/
        <VerbActionName>/
          <VerbActionName>Query.cs
          <VerbActionName>QueryHandler.cs
          <VerbActionName>QueryResult.cs
```

---

## Implementing a Command

### 1. Define the command (input)

```csharp
// Commands/PostCreateTournament/CreateTournamentCommand.cs
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

- Commands are **sealed records** for immutability.
- Implement `ICommand<TResult>` for commands that return data, or `ICommand` for fire-and-forget.

### 2. Define the result (output)

```csharp
// Commands/PostCreateTournament/CreateTournamentCommandResult.cs
public class CreateTournamentCommandResult : ICommandResult
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

- Must implement `ICommandResult`.

### 3. Implement the handler

```csharp
// Commands/PostCreateTournament/CreateTournamentCommandHandler.cs
public class CreateTournamentCommandHandler(
    IRepository<Tournament> repository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser)
    : ICommandHandler<CreateTournamentCommand, CreateTournamentCommandResult>
{
    public async Task<Result<CreateTournamentCommandResult>> HandleAsync(
        CreateTournamentCommand command, CancellationToken cancellationToken)
    {
        var entity = new Tournament
        {
            Name        = command.Name,
            Description = command.Description,
            StartDate   = command.StartDate,
            EndDate     = command.EndDate,
            LogoUrl     = command.LogoUrl,
            BannerUrl   = command.BannerUrl,
            CreatedBy   = currentUser.Username
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

**Rules:**
- Always inject `IRepository<T>` and `ICoreUnitOfWork`.
- Inject `ICurrentUser` to stamp `CreatedBy` / `UpdatedBy`.
- Call `repository.SaveAsync` / `UpdateAsync` to stage changes, then `unitOfWork.SaveChangesAsync` once at the end to commit.
- Return `Result.Fail<TResult>(...)` on any failure; never throw for expected errors.

---

## Implementing a Query

```csharp
// Queries/GetTournaments/GetTournamentsQuery.cs
public sealed record GetTournamentsQuery(int Page, int PerPage) : IQuery<GetTournamentsQueryResult>;

// Queries/GetTournaments/GetTournamentsQueryResult.cs
public class GetTournamentsQueryResult : IQueryResult
{
    public PaginationResult<TournamentDto> Tournaments { get; set; } = null!;
}

// Queries/GetTournaments/GetTournamentsQueryHandler.cs
public class GetTournamentsQueryHandler(IRepository<Tournament> repository)
    : IQueryHandler<GetTournamentsQuery, GetTournamentsQueryResult>
{
    public async Task<Result<GetTournamentsQueryResult>> HandleAsync(
        GetTournamentsQuery query, CancellationToken cancellationToken)
    {
        var spec   = new GetTournamentsSpec(query.Page, query.PerPage);
        var paged  = await repository.GetPaginatedAsync(spec, cancellationToken);
        // map paged.Data to DTOs if needed
        return Result.Success(new GetTournamentsQueryResult { Tournaments = paged });
    }
}
```

---

## Existing Commands / Queries

### Tournaments module

| Aggregate | Operation | Type | Route |
|---|---|---|---|
| Tournaments | `PostCreateTournament` | Command | `POST /api/v1/tournaments` |
| Tournaments | `PutUpdateTournament` | Command | `PUT /api/v1/tournaments` |
| Tournaments | `DeleteTournament` | Command | `DELETE /api/v1/tournaments/{id}` |
| Tournaments | `GetAllTournaments` | Query | `GET /api/v1/tournaments` |
| Tournaments | `GetTournamentById` | Query | `GET /api/v1/tournaments/{id}` |
| TeamParticipations | `PostCreateTeamParticipation` | Command | `POST /api/v1/team-participations` |
| TeamParticipations | `RegisterTeams` *(batch)* | Command | `POST /api/v1/tournaments/{tournamentId}/team-participations` |
| TeamParticipations | `PutUpdateTeamParticipation` | Command | `PUT /api/v1/team-participations` |
| TeamParticipations | `DeleteTeamParticipation` | Command | `DELETE /api/v1/team-participations/{id}` |
| TeamParticipations | `GetAllTeamParticipations` | Query | `GET /api/v1/team-participations` |
| TeamParticipations | `GetTeamParticipationById` | Query | `GET /api/v1/team-participations/{id}` |
| RoundsClassified | `GetAllRoundsClassified` | Query | `GET /api/v1/rounds-classified` |
| RoundsClassified | `GetRoundsClassifiedById` | Query | `GET /api/v1/rounds-classified/{id}` |

### Teams module

| Aggregate | Operation | Type | Route |
|---|---|---|---|
| Teams | `PostCreateTeam` | Command | `POST /api/v1/teams` |
| Teams | `PutUpdateTeam` | Command | `PUT /api/v1/teams` |
| Teams | `DeleteTeam` | Command | `DELETE /api/v1/teams/{id}` |
| Teams | `GetAllTeams` | Query | `GET /api/v1/teams` |
| Teams | `GetTeamById` | Query | `GET /api/v1/teams/{id}` |
| Players | `PostCreatePlayer` | Command | `POST /api/v1/players` |
| Players | `PutUpdatePlayer` | Command | `PUT /api/v1/players` |
| Players | `DeletePlayer` | Command | `DELETE /api/v1/players/{id}` |
| Players | `GetAllPlayers` | Query | `GET /api/v1/players` |
| Players | `GetPlayerById` | Query | `GET /api/v1/players/{id}` |
| Rosters | `PostCreateRoster` | Command | `POST /api/v1/rosters` |
| Rosters | `EnrollPlayers` *(batch)* | Command | `POST /api/v1/team-participations/{teamParticipationId}/rosters` |
| Rosters | `PutUpdateRoster` | Command | `PUT /api/v1/rosters` |
| Rosters | `DeleteRoster` | Command | `DELETE /api/v1/rosters/{id}` |
| Rosters | `GetAllRosters` | Query | `GET /api/v1/rosters` |
| Rosters | `GetRosterById` | Query | `GET /api/v1/rosters/{id}` |

### Matches module

| Aggregate | Operation | Type | Route |
|---|---|---|---|
| Matches | `PostCreateMatch` | Command | `POST /api/v1/matches` |
| Matches | `PutUpdateMatch` | Command | `PUT /api/v1/matches` |
| Matches | `DeleteMatch` | Command | `DELETE /api/v1/matches/{id}` |
| Matches | `GetAllMatches` | Query | `GET /api/v1/matches` |
| Matches | `GetMatchById` | Query | `GET /api/v1/matches/{id}` |
| Events | `PostCreateEvent` | Command | `POST /api/v1/events` |
| Events | `PutUpdateEvent` | Command | `PUT /api/v1/events` |
| Events | `DeleteEvent` | Command | `DELETE /api/v1/events/{id}` |
| Events | `GetAllEvents` | Query | `GET /api/v1/events` |
| Events | `GetEventById` | Query | `GET /api/v1/events/{id}` |

