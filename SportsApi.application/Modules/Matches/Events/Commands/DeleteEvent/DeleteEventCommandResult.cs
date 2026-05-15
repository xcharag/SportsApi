using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Matches.Events.Commands.DeleteEvent;

public class DeleteEventCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

