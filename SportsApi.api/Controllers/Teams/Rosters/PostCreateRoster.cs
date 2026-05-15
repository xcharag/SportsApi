using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Rosters.Commands.PostCreateRoster;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Rosters;

[ApiController]
[Authorize]
[DynamicPermission]
public class PostCreateRoster(IMediator mediator) : EndpointBaseAsync
    .WithRequest<CreateRosterCommand>
    .WithActionResult<CreateRosterCommandResult>
{
    [HttpPost("api/v1/rosters")]
    [SwaggerOperation(Tags = ["Rosters"])]
    [ProducesResponseType((int)HttpStatusCode.Created,    Type = typeof(CreateRosterCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationProblemDetails))]
    public override async Task<ActionResult<CreateRosterCommandResult>> HandleAsync(
        [FromBody] CreateRosterCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<CreateRosterCommand, CreateRosterCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to create roster", detail: result.Error, statusCode: (int)HttpStatusCode.BadRequest);

        return CreatedAtRoute("GetRosterById", new { id = result.Value.Id }, result.Value);
    }
}

