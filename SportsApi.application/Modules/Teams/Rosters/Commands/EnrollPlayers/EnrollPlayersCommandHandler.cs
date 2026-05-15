using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Modules.Teams;

namespace SportsApi.application.Modules.Teams.Rosters.Commands.EnrollPlayers;

public class EnrollPlayersCommandHandler(
    IRepository<Roster> rosterRepository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<EnrollPlayersCommand, EnrollPlayersCommandResult>
{
    public async Task<Result<EnrollPlayersCommandResult>> HandleAsync(
        EnrollPlayersCommand command,
        CancellationToken cancellationToken)
    {
        var username  = currentUser.Username;
        var createdAt = DateTime.UtcNow;

        var rosters   = new List<Roster>();
        var summaries = new List<EnrolledPlayerSummary>();

        foreach (var item in command.Players)
        {
            var rosterId = Guid.NewGuid();

            rosters.Add(new Roster
            {
                Id                  = rosterId,
                PlayerId            = item.PlayerId,
                TeamParticipationId = command.TeamParticipationId,
                ShirtNumber         = item.ShirtNumber,
                ShirtName           = item.ShirtName,
                CreatedBy           = username,
                CreatedAt           = createdAt
            });

            summaries.Add(new EnrolledPlayerSummary
            {
                RosterId    = rosterId,
                PlayerId    = item.PlayerId,
                ShirtNumber = item.ShirtNumber,
                ShirtName   = item.ShirtName
            });
        }

        var saveResult = await rosterRepository.SaveAsync(rosters.ToArray(), cancellationToken);
        if (saveResult.IsFailure)
            return Result.Fail<EnrollPlayersCommandResult>(saveResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new EnrollPlayersCommandResult
        {
            EnrolledCount = summaries.Count,
            Players       = summaries
        });
    }
}

