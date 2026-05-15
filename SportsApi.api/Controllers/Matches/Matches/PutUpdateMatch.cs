using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Matches.Matches.Commands.PutUpdateMatch;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Matches.Matches;

[ApiController]
[Authorize]
[DynamicPermission]
public class PutUpdateMatch(IMediator mediator) : EndpointBaseAsync
    .WithRequest<UpdateMatchCommand>
    .WithActionResult<UpdateMatchCommandResult>
{
    [HttpPut("api/v1/matches/{id}")]
    [SwaggerOperation(Tags = ["Matches"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(UpdateMatchCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<UpdateMatchCommandResult>> HandleAsync(
        [FromBody] UpdateMatchCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<UpdateMatchCommand, UpdateMatchCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

