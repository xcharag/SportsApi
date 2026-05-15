using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentStandings;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using SportsApi.infrastructure.Services.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[ApiController]
[AllowAnonymous]
[SkipDynamicPermission]
public class GetTournamentStandings(IMediator mediator) : EndpointBaseAsync
    .WithRequest<TournamentStandingsQuery>
    .WithActionResult<TournamentStandingsQueryResult>
{
    [HttpGet("api/v1/tournaments/{tournamentId}/standings")]
    [SwaggerOperation(
        Summary     = "Get group-stage standings",
        Description = "Computes W/D/L/GF/GA/GD/Pts for each team in every group. Optionally filter by groupKey.",
        Tags        = ["Tournaments"])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TournamentStandingsQueryResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<ActionResult<TournamentStandingsQueryResult>> HandleAsync(
        [FromQuery] TournamentStandingsQuery request,
        CancellationToken cancellationToken = default)
    {
        if (RouteData.Values.TryGetValue("tournamentId", out var routeId)
            && Guid.TryParse(routeId?.ToString(), out var id))
        {
            request.TournamentId = id;
        }

        var result = await mediator.SendQueryAsync<TournamentStandingsQuery, TournamentStandingsQueryResult>(
            request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to compute standings", detail: result.Error,
                statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}
