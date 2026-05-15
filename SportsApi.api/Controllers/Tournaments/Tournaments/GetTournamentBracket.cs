using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentBracket;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using SportsApi.infrastructure.Services.Auth;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[ApiController]
[AllowAnonymous]
[SkipDynamicPermission]
public class GetTournamentBracket(IMediator mediator) : EndpointBaseAsync
    .WithoutRequest
    .WithActionResult<TournamentBracketQueryResult>
{
    [HttpGet("api/v1/tournaments/{tournamentId}/bracket")]
    [SwaggerOperation(
        Summary     = "Get knockout bracket",
        Description = "Returns the full knockout bracket from R16 through the Final, organized by round and slot.",
        Tags        = ["Tournaments"])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TournamentBracketQueryResult))]
    public override async Task<ActionResult<TournamentBracketQueryResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        if (!RouteData.Values.TryGetValue("tournamentId", out var routeId)
            || !Guid.TryParse(routeId?.ToString(), out var id))
        {
            return BadRequest(new { error = "Invalid tournamentId" });
        }

        var query  = new TournamentBracketQuery { TournamentId = id };
        var result = await mediator.SendQueryAsync<TournamentBracketQuery, TournamentBracketQueryResult>(
            query, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to build bracket", detail: result.Error,
                statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}
