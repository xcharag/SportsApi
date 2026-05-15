using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentTopScorers;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using SportsApi.infrastructure.Services.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[ApiController]
[AllowAnonymous]
[SkipDynamicPermission]
public class GetTournamentTopScorers(IMediator mediator) : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<TournamentTopScorersQueryResult>
{
    [HttpGet("api/v1/tournaments/{tournamentId}/top-scorers")]
    [SwaggerOperation(
        Summary     = "Get tournament top scorers",
        Description = "Returns the ranked list of top scorers for a tournament. Use ?limit=N to set the maximum results (default 10).",
        Tags        = ["Tournaments"])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TournamentTopScorersQueryResult))]
    public override async Task<ActionResult<TournamentTopScorersQueryResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        if (!RouteData.Values.TryGetValue("tournamentId", out var routeId)
            || !Guid.TryParse(routeId?.ToString(), out var id))
        {
            return BadRequest(new { error = "Invalid tournamentId" });
        }

        int limit = 10;
        if (Request.Query.TryGetValue("limit", out var limitStr) && int.TryParse(limitStr, out var parsed))
            limit = parsed;

        var query  = new TournamentTopScorersQuery { TournamentId = id, Limit = limit };
        var result = await mediator.SendQueryAsync<TournamentTopScorersQuery, TournamentTopScorersQueryResult>(
            query, cancellationToken);

        if (result.IsFailure)
            return Problem(detail: result.Error, statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}
