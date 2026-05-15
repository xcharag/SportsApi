using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.PostCreateTeamParticipation;

public sealed record CreateTeamParticipationCommand : ICommand<CreateTeamParticipationCommandResult>
{
    public string  Name         { get; set; } = string.Empty;
    public string? LogoUrl      { get; set; }
    public Guid    TeamId       { get; set; }
    public Guid    TournamentId { get; set; }
}

