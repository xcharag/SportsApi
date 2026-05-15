using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Players.Commands.DeletePlayer;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Players;

[ApiController]
[Authorize]
[DynamicPermission]
public class DeletePlayer(IMediator mediator) : EndpointBaseAsync
    .WithRequest<DeletePlayerCommand>
    .WithActionResult<DeletePlayerCommandResult>
{
    [HttpDelete("api/v1/players/{id}")]
    [SwaggerOperation(Tags = ["Players"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(DeletePlayerCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<DeletePlayerCommandResult>> HandleAsync(
        [FromRoute] DeletePlayerCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<DeletePlayerCommand, DeletePlayerCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

