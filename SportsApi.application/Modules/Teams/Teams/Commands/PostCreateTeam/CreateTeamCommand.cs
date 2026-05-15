using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Teams.Commands.PostCreateTeam;

public sealed record CreateTeamCommand : ICommand<CreateTeamCommandResult>
{
    public string DefaultName { get; set; } = string.Empty;
    public string? DefaultLogoUrl { get; set; }
}

