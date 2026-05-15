using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Players.Queries.GetPlayerStats;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using SportsApi.infrastructure.Services.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Players;

[ApiController]
[AllowAnonymous]
[SkipDynamicPermission]
public class GetPlayerStats(IMediator mediator) : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<PlayerStatsQueryResult>
{
    [HttpGet("api/v1/players/{playerId}/stats")]
    [SwaggerOperation(
        Summary     = "Get player stats",
        Description = "Returns career and per-tournament statistics for a player.",
        Tags        = ["Players"])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlayerStatsQueryResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<ActionResult<PlayerStatsQueryResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        if (!RouteData.Values.TryGetValue("playerId", out var routeId)
            || !Guid.TryParse(routeId?.ToString(), out var id))
        {
            return BadRequest(new { error = "Invalid playerId" });
        }

        var query  = new PlayerStatsQuery { PlayerId = id };
        var result = await mediator.SendQueryAsync<PlayerStatsQuery, PlayerStatsQueryResult>(
            query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.ErrorKey == "PLAYER_NOT_FOUND") return NotFound();
            return Problem(detail: result.Error, statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(result.Value);
    }
}
