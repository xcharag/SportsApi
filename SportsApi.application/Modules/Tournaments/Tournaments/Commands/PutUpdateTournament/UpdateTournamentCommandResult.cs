using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.PutUpdateTournament;

public class UpdateTournamentCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}