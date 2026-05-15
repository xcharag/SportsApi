using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Matches.Events.Commands.PutUpdateEvent;

public class UpdateEventCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

