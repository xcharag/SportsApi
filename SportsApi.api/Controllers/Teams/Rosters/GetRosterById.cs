using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Rosters.Queries.GetRosterById;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Rosters;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetRosterById(IMediator mediator) : EndpointBaseAsync
    .WithRequest<RosterByIdQuery>
    .WithActionResult<RosterByIdQueryResult>
{
    [HttpGet("api/v1/rosters/{id}", Name = "GetRosterById")]
    [SwaggerOperation(Tags = ["Rosters"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(RosterByIdQueryResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<RosterByIdQueryResult>> HandleAsync(
        [FromRoute] RosterByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<RosterByIdQuery, RosterByIdQueryResult>(
            new RosterByIdQuery { Id = request.Id }, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

