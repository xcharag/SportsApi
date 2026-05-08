using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Queries.GetTournamentById;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetTournamentById(IMediator mediator) : EndpointBaseAsync
    .WithRequest<TournamentByIdQuery>
    .WithActionResult<TournamentByIdQueryResult>
{
    [HttpGet("api/v1/tournaments/{id}", Name = "GetTournamentById")]
    [SwaggerOperation(Tags = ["Tournaments"])]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(TournamentByIdQueryResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<TournamentByIdQueryResult>> HandleAsync(
        [FromRoute] TournamentByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<TournamentByIdQuery, TournamentByIdQueryResult>(
            new TournamentByIdQuery { Id = request.Id }, cancellationToken);
        
        if (result.IsFailure) return NotFound(new {error = result.Error});
        
        return Ok(result.Value);
    }
}