using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Matches.Events.Commands.DeleteEvent;

public class DeleteEventCommand : ICommand<DeleteEventCommandResult>
{
    public Guid Id { get; set; }
    public bool HardDelete { get; set; } = false;
}

