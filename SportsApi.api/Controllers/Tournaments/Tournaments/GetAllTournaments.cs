using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetAllTournaments;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[ApiController]
[AllowAnonymous]
public class GetAllTournaments(IMediator mediator) : EndpointBaseAsync
    .WithRequest<AllTournamentsQuery>
    .WithActionResult<AllTournamentsQueryResult>
{
    [HttpGet("api/v1/tournaments")]
    [SwaggerOperation(Tags = ["Tournaments"])]
    public override async Task<ActionResult<AllTournamentsQueryResult>> HandleAsync(
        [FromQuery] AllTournamentsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator
            .SendQueryAsync<AllTournamentsQuery, AllTournamentsQueryResult>(
                request,
                cancellationToken
            );

        if (result.IsFailure)
            return Problem(
                title: "Failed to retrieve tournaments",
                detail: result.Error,
                statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}