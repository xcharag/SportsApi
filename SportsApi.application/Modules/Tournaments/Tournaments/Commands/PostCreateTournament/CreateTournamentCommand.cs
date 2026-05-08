using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Tournaments.Tournaments.Commands.PostCreateTournament;

public sealed record CreateTournamentCommand : ICommand<CreateTournamentCommandResult>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
}