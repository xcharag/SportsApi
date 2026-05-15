using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Players.Commands.PostCreatePlayer;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Players;

[ApiController]
[Authorize]
[DynamicPermission]
public class PostCreatePlayer(IMediator mediator) : EndpointBaseAsync
    .WithRequest<CreatePlayerCommand>
    .WithActionResult<CreatePlayerCommandResult>
{
    [HttpPost("api/v1/players")]
    [SwaggerOperation(Tags = ["Players"])]
    [ProducesResponseType((int)HttpStatusCode.Created,    Type = typeof(CreatePlayerCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationProblemDetails))]
    public override async Task<ActionResult<CreatePlayerCommandResult>> HandleAsync(
        [FromBody] CreatePlayerCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<CreatePlayerCommand, CreatePlayerCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to create player", detail: result.Error, statusCode: (int)HttpStatusCode.BadRequest);

        return CreatedAtRoute("GetPlayerById", new { id = result.Value.Id }, result.Value);
    }
}

