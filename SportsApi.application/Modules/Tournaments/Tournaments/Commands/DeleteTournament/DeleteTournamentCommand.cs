using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.DeleteTournament;

public class DeleteTournamentCommand : ICommand<DeleteTournamentCommandResult>
{
    public Guid Id { get; set; }
    public bool HardDelete { get; set; } = false;
}