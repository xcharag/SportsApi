using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Enums.Status;

namespace SportsApi.application.Modules.Matches.Matches.Commands.PutUpdateMatch;

public class UpdateMatchCommand : ICommand<UpdateMatchCommandResult>
{
    public Guid         Id            { get; set; }
    public int?         MatchDay      { get; set; }
    public DateTime?    MatchDate     { get; set; }
    public string?      Field         { get; set; }
    public string?      Location      { get; set; }
    public MatchStatus? Status        { get; set; }
    public int?         ScoreHomeTeam { get; set; }
    public int?         ScoreAwayTeam { get; set; }
    public Guid?        NewMatchId    { get; set; }
}

