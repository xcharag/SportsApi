using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Teams.Queries.GetTeamById;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Teams;

[ApiController]
[AllowAnonymous]
public class GetTeamById(IMediator mediator) : EndpointBaseAsync
    .WithRequest<TeamByIdQuery>
    .WithActionResult<TeamByIdQueryResult>
{
    [HttpGet("api/v1/teams/{id}", Name = "GetTeamById")]
    [SwaggerOperation(Tags = ["Teams"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(TeamByIdQueryResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<TeamByIdQueryResult>> HandleAsync(
        [FromRoute] TeamByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<TeamByIdQuery, TeamByIdQueryResult>(
            new TeamByIdQuery { Id = request.Id }, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

