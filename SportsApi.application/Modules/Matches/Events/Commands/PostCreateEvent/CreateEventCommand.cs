using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Types;

namespace SportsApi.application.Modules.Matches.Events.Commands.PostCreateEvent;

public sealed record CreateEventCommand : ICommand<CreateEventCommandResult>
{
    public int         Minute      { get; set; }
    public FavorableTo FavorableTo { get; set; }
    public EventType   EventType   { get; set; }
    public Guid        RosterId    { get; set; }
    public Guid        MatchId     { get; set; }
}

