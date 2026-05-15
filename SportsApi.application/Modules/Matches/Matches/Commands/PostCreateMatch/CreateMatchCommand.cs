using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Enums.Status;

namespace SportsApi.application.Modules.Matches.Matches.Commands.PostCreateMatch;

public sealed record CreateMatchCommand : ICommand<CreateMatchCommandResult>
{
    public int        MatchDay      { get; set; }
    public DateTime   MatchDate     { get; set; }
    public string?    Field         { get; set; }
    public string?    Location      { get; set; }
    public MatchStatus Status       { get; set; } = MatchStatus.Pending;
    public Guid       HomeTeamId    { get; set; }
    public Guid       AwayTeamId    { get; set; }
    public Guid?      NewMatchId    { get; set; }
}

