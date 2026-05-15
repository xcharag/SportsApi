using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Teams.Queries.GetTeamProfile;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using SportsApi.infrastructure.Services.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Teams;

[ApiController]
[AllowAnonymous]
[SkipDynamicPermission]
public class GetTeamProfile(IMediator mediator) : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<TeamProfileQueryResult>
{
    [HttpGet("api/v1/teams/{teamId}/profile")]
    [SwaggerOperation(
        Summary     = "Get team profile",
        Description = "Returns tournament history, championship wins, historic top scorer, and career stats for a team.",
        Tags        = ["Teams"])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeamProfileQueryResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<ActionResult<TeamProfileQueryResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        if (!RouteData.Values.TryGetValue("teamId", out var routeId)
            || !Guid.TryParse(routeId?.ToString(), out var id))
        {
            return BadRequest(new { error = "Invalid teamId" });
        }

        var query  = new TeamProfileQuery { TeamId = id };
        var result = await mediator.SendQueryAsync<TeamProfileQuery, TeamProfileQueryResult>(
            query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.ErrorKey == "TEAM_NOT_FOUND") return NotFound();
            return Problem(detail: result.Error, statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(result.Value);
    }
}
