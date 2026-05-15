using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Matches.Events.Commands.PostCreateEvent;

public class CreateEventCommandResult : ICommandResult
{
    public Guid Id { get; set; }
}

