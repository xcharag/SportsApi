using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Rosters.Commands.EnrollPlayers;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Rosters;

/// <summary>
/// Enrolls multiple players into a TeamParticipation in one call.
/// Creates one Roster record per player.
/// </summary>
[ApiController]
[Authorize]
[DynamicPermission]
public class PostRosters(IMediator mediator) : EndpointBaseAsync
    .WithRequest<EnrollPlayersCommand>
    .WithActionResult<EnrollPlayersCommandResult>
{
    [HttpPost("api/v1/team-participations/{teamParticipationId}/rosters")]
    [SwaggerOperation(
        Summary = "Enroll players into a team-in-tournament",
        Description = "Bulk-enrolls one or more players into a TeamParticipation, creating a Roster entry for each. " +
                      "A Roster ties a Player to a specific team in a specific tournament season.",
        Tags = ["Rosters"])]
    [ProducesResponseType((int)HttpStatusCode.Created, Type = typeof(EnrollPlayersCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationProblemDetails))]
    public override async Task<ActionResult<EnrollPlayersCommandResult>> HandleAsync(
        [FromBody] EnrollPlayersCommand request,
        CancellationToken cancellationToken = default)
    {
        // Inject the route segment as the authoritative TeamParticipationId
        if (RouteData.Values.TryGetValue("teamParticipationId", out var routeId)
            && Guid.TryParse(routeId?.ToString(), out var teamParticipationId))
        {
            request.TeamParticipationId = teamParticipationId;
        }

        var result = await mediator.SendCommandAsync<EnrollPlayersCommand, EnrollPlayersCommandResult>(
            request, cancellationToken);

        if (result.IsFailure)
            return Problem(
                title:      "Failed to enroll players",
                detail:     result.Error,
                statusCode: (int)HttpStatusCode.BadRequest);

        return StatusCode((int)HttpStatusCode.Created, result.Value);
    }
}

