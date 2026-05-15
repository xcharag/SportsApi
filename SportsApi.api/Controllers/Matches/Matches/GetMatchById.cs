using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Matches.Matches.Queries.GetMatchById;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Matches.Matches;

[ApiController]
[AllowAnonymous]
public class GetMatchById(IMediator mediator) : EndpointBaseAsync
    .WithRequest<MatchByIdQuery>
    .WithActionResult<MatchByIdQueryResult>
{
    [HttpGet("api/v1/matches/{id}", Name = "GetMatchById")]
    [SwaggerOperation(Tags = ["Matches"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(MatchByIdQueryResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<MatchByIdQueryResult>> HandleAsync(
        [FromRoute] MatchByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<MatchByIdQuery, MatchByIdQueryResult>(
            new MatchByIdQuery { Id = request.Id }, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

