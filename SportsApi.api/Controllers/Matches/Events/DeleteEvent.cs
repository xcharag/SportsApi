using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Matches.Events.Commands.DeleteEvent;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Matches.Events;

[ApiController]
[Authorize]
[DynamicPermission]
public class DeleteEvent(IMediator mediator) : EndpointBaseAsync
    .WithRequest<DeleteEventCommand>
    .WithActionResult<DeleteEventCommandResult>
{
    [HttpDelete("api/v1/events/{id}")]
    [SwaggerOperation(Tags = ["Events"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(DeleteEventCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<DeleteEventCommandResult>> HandleAsync(
        [FromRoute] DeleteEventCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<DeleteEventCommand, DeleteEventCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

