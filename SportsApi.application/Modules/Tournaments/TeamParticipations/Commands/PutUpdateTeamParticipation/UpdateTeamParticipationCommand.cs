using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.PutUpdateTeamParticipation;

public class UpdateTeamParticipationCommand : ICommand<UpdateTeamParticipationCommandResult>
{
    public Guid    Id      { get; set; }
    public string? Name    { get; set; }
    public string? LogoUrl { get; set; }
}

