using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Matches.Events.Queries.GetEventById;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Matches.Events;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetEventById(IMediator mediator) : EndpointBaseAsync
    .WithRequest<EventByIdQuery>
    .WithActionResult<EventByIdQueryResult>
{
    [HttpGet("api/v1/events/{id}", Name = "GetEventById")]
    [SwaggerOperation(Tags = ["Events"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(EventByIdQueryResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<EventByIdQueryResult>> HandleAsync(
        [FromRoute] EventByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<EventByIdQuery, EventByIdQueryResult>(
            new EventByIdQuery { Id = request.Id }, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

