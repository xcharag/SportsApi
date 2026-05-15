using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Matches.Matches.Commands.DeleteMatch;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Matches.Matches;

[ApiController]
[Authorize]
[DynamicPermission]
public class DeleteMatch(IMediator mediator) : EndpointBaseAsync
    .WithRequest<DeleteMatchCommand>
    .WithActionResult<DeleteMatchCommandResult>
{
    [HttpDelete("api/v1/matches/{id}")]
    [SwaggerOperation(Tags = ["Matches"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(DeleteMatchCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<DeleteMatchCommandResult>> HandleAsync(
        [FromRoute] DeleteMatchCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<DeleteMatchCommand, DeleteMatchCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

