using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Players.Commands.PutUpdatePlayer;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Players;

[ApiController]
[Authorize]
[DynamicPermission]
public class PutUpdatePlayer(IMediator mediator) : EndpointBaseAsync
    .WithRequest<UpdatePlayerCommand>
    .WithActionResult<UpdatePlayerCommandResult>
{
    [HttpPut("api/v1/players/{id}")]
    [SwaggerOperation(Tags = ["Players"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(UpdatePlayerCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<UpdatePlayerCommandResult>> HandleAsync(
        [FromBody] UpdatePlayerCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<UpdatePlayerCommand, UpdatePlayerCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

