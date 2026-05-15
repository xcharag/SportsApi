using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.RegisterTeams;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.TeamParticipations;

/// <summary>
/// Registers multiple teams into a tournament in one call.
/// Creates one TeamParticipation + one RoundsClassified (Group stage) per team.
/// </summary>
[ApiController]
[Authorize]
[DynamicPermission]
public class PostTeamParticipations(IMediator mediator) : EndpointBaseAsync
    .WithRequest<RegisterTeamsCommand>
    .WithActionResult<RegisterTeamsCommandResult>
{
    [HttpPost("api/v1/tournaments/{tournamentId}/team-participations")]
    [SwaggerOperation(
        Summary = "Register teams to a tournament",
        Description = "Bulk-registers one or more teams into a tournament. Each team gets a TeamParticipation and a group-stage RoundsClassified entry.",
        Tags = ["TeamParticipations"])]
    [ProducesResponseType((int)HttpStatusCode.Created, Type = typeof(RegisterTeamsCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationProblemDetails))]
    public override async Task<ActionResult<RegisterTeamsCommandResult>> HandleAsync(
        [FromBody] RegisterTeamsCommand request,
        CancellationToken cancellationToken = default)
    {
        // Inject the route segment as the authoritative TournamentId
        if (RouteData.Values.TryGetValue("tournamentId", out var routeId)
            && Guid.TryParse(routeId?.ToString(), out var tournamentId))
        {
            request.TournamentId = tournamentId;
        }

        var result = await mediator.SendCommandAsync<RegisterTeamsCommand, RegisterTeamsCommandResult>(
            request, cancellationToken);

        if (result.IsFailure)
            return Problem(
                title:      "Failed to register teams",
                detail:     result.Error,
                statusCode: (int)HttpStatusCode.BadRequest);

        return StatusCode((int)HttpStatusCode.Created, result.Value);
    }
}

