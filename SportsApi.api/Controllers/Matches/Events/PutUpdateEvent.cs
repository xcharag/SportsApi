using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Matches.Events.Commands.PutUpdateEvent;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Matches.Events;

[ApiController]
[Authorize]
[DynamicPermission]
public class PutUpdateEvent(IMediator mediator) : EndpointBaseAsync
    .WithRequest<UpdateEventCommand>
    .WithActionResult<UpdateEventCommandResult>
{
    [HttpPut("api/v1/events/{id}")]
    [SwaggerOperation(Tags = ["Events"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(UpdateEventCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<UpdateEventCommandResult>> HandleAsync(
        [FromBody] UpdateEventCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<UpdateEventCommand, UpdateEventCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

