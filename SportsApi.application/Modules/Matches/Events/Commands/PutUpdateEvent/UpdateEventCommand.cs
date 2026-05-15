using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Enums;
using SportsApi.domain.Enums.Types;

namespace SportsApi.application.Modules.Matches.Events.Commands.PutUpdateEvent;

public class UpdateEventCommand : ICommand<UpdateEventCommandResult>
{
    public Guid         Id          { get; set; }
    public int?         Minute      { get; set; }
    public FavorableTo? FavorableTo { get; set; }
    public EventType?   EventType   { get; set; }
}

