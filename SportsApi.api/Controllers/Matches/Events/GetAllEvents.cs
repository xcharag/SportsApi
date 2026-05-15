using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Matches.Events.Queries.GetAllEvents;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Matches.Events;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetAllEvents(IMediator mediator) : EndpointBaseAsync
    .WithRequest<AllEventsQuery>
    .WithActionResult<AllEventsQueryResult>
{
    [HttpGet("api/v1/events")]
    [SwaggerOperation(Tags = ["Events"])]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AllEventsQueryResult))]
    public override async Task<ActionResult<AllEventsQueryResult>> HandleAsync(
        [FromQuery] AllEventsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<AllEventsQuery, AllEventsQueryResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to retrieve events", detail: result.Error, statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}

