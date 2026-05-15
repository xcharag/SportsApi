using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Matches.Events.Commands.PostCreateEvent;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Matches.Events;

[ApiController]
[Authorize]
[DynamicPermission]
public class PostCreateEvent(IMediator mediator) : EndpointBaseAsync
    .WithRequest<CreateEventCommand>
    .WithActionResult<CreateEventCommandResult>
{
    [HttpPost("api/v1/events")]
    [SwaggerOperation(Tags = ["Events"])]
    [ProducesResponseType((int)HttpStatusCode.Created,    Type = typeof(CreateEventCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationProblemDetails))]
    public override async Task<ActionResult<CreateEventCommandResult>> HandleAsync(
        [FromBody] CreateEventCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<CreateEventCommand, CreateEventCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to create event", detail: result.Error, statusCode: (int)HttpStatusCode.BadRequest);

        return CreatedAtRoute("GetEventById", new { id = result.Value.Id }, result.Value);
    }
}

