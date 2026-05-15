using SportsApi.domain.Abstractions.Auth;
using SportsApi.domain.Abstractions.Dtos;
using SportsApi.domain.Abstractions.Messaging.Commands;
using SportsApi.domain.Abstractions.Persistence;
using SportsApi.domain.Enums;
using DomainTournaments = SportsApi.domain.Modules.Tournaments;

namespace SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.RegisterTeams;

public class RegisterTeamsCommandHandler(
    IRepository<DomainTournaments.TeamParticipation> participationRepository,
    IRepository<DomainTournaments.RoundsClassified> roundsRepository,
    ICoreUnitOfWork unitOfWork,
    ICurrentUser currentUser) : ICommandHandler<RegisterTeamsCommand, RegisterTeamsCommandResult>
{
    public async Task<Result<RegisterTeamsCommandResult>> HandleAsync(
        RegisterTeamsCommand command,
        CancellationToken cancellationToken)
    {
        var username  = currentUser.Username;
        var createdAt = DateTime.UtcNow;

        var participations = new List<DomainTournaments.TeamParticipation>();
        var rounds         = new List<DomainTournaments.RoundsClassified>();
        var summaries      = new List<RegisteredTeamSummary>();

        foreach (var item in command.Teams)
        {
            var participationId = Guid.NewGuid();

            participations.Add(new DomainTournaments.TeamParticipation
            {
                Id           = participationId,
                TeamId       = item.TeamId,
                TournamentId = command.TournamentId,
                Name         = item.Name,
                LogoUrl      = item.LogoUrl,
                CreatedBy    = username,
                CreatedAt    = createdAt
            });

            rounds.Add(new DomainTournaments.RoundsClassified
            {
                Id                  = Guid.NewGuid(),
                TeamParticipationId = participationId,
                Round               = MatchRound.Group,
                RoundKey            = item.RoundKey,
                GroupPosition       = item.GroupPosition,
                NextRoundKey        = item.NextRoundKey,
                CreatedBy           = username,
                CreatedAt           = createdAt
            });

            summaries.Add(new RegisteredTeamSummary
            {
                TeamParticipationId = participationId,
                TeamId              = item.TeamId,
                Name                = item.Name,
                RoundKey            = item.RoundKey
            });
        }

        var saveParticipations = await participationRepository.SaveAsync(participations.ToArray(), cancellationToken);
        if (saveParticipations.IsFailure)
            return Result.Fail<RegisterTeamsCommandResult>(saveParticipations.Error);

        var saveRounds = await roundsRepository.SaveAsync(rounds.ToArray(), cancellationToken);
        if (saveRounds.IsFailure)
            return Result.Fail<RegisterTeamsCommandResult>(saveRounds.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RegisterTeamsCommandResult
        {
            RegisteredCount = summaries.Count,
            Teams           = summaries
        });
    }
}



