using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Players.Queries.GetPlayerProfile;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using SportsApi.infrastructure.Services.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Players;

[ApiController]
[AllowAnonymous]
[SkipDynamicPermission]
public class GetPlayerProfile(IMediator mediator) : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<PlayerProfileQueryResult>
{
    [HttpGet("api/v1/players/{playerId}/profile")]
    [SwaggerOperation(
        Summary     = "Get player profile",
        Description = "Returns all teams a player has participated in, with career stats and per-match event history.",
        Tags        = ["Players"])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PlayerProfileQueryResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<ActionResult<PlayerProfileQueryResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        if (!RouteData.Values.TryGetValue("playerId", out var routeId)
            || !Guid.TryParse(routeId?.ToString(), out var id))
        {
            return BadRequest(new { error = "Invalid playerId" });
        }

        var query  = new PlayerProfileQuery { PlayerId = id };
        var result = await mediator.SendQueryAsync<PlayerProfileQuery, PlayerProfileQueryResult>(
            query, cancellationToken);

        if (result.IsFailure)
        {
            if (result.ErrorKey == "PLAYER_NOT_FOUND") return NotFound();
            return Problem(detail: result.Error, statusCode: StatusCodes.Status500InternalServerError);
        }

        return Ok(result.Value);
    }
}
