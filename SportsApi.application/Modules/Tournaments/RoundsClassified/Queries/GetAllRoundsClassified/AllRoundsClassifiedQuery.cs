using SportsApi.domain.Abstractions.Messaging.Queries;
using SportsApi.domain.Enums;

namespace SportsApi.application.Modules.Tournaments.RoundsClassified.Queries.GetAllRoundsClassified;

public class AllRoundsClassifiedQuery : IQuery<AllRoundsClassifiedQueryResult>
{
    public int         Page                { get; set; } = 1;
    public int         PerPage             { get; set; } = 20;
    public Guid?       TeamParticipationId { get; set; }
    public Guid?       TournamentId        { get; set; }
    public MatchRound? Round               { get; set; }
}

