using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Teams.Teams.Commands.PutUpdateTeam;

public class UpdateTeamCommand : ICommand<UpdateTeamCommandResult>
{
    public Guid    Id             { get; set; }
    public string? DefaultName    { get; set; }
    public string? DefaultLogoUrl { get; set; }
}

