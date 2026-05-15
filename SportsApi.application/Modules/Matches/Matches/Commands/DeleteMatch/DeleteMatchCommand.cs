using SportsApi.domain.Abstractions.Messaging.Commands;

namespace SportsApi.application.Modules.Matches.Matches.Commands.DeleteMatch;

public class DeleteMatchCommand : ICommand<DeleteMatchCommandResult>
{
    public Guid Id { get; set; }
    public bool HardDelete { get; set; } = false;
}

