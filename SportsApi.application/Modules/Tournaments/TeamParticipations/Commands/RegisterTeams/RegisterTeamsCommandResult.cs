using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.RegisterTeams;

public class RegisterTeamsCommandResult : ICommandResult
{
    public int RegisteredCount { get; set; }
    public List<RegisteredTeamSummary> Teams { get; set; } = [];
}

public class RegisteredTeamSummary
{
    public Guid   TeamParticipationId { get; set; }
    public Guid   TeamId              { get; set; }
    public string Name                { get; set; } = string.Empty;
    public string RoundKey            { get; set; } = string.Empty;
}

