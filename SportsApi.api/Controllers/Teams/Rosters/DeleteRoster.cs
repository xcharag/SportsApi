using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Rosters.Commands.DeleteRoster;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Rosters;

[ApiController]
[Authorize]
[DynamicPermission]
public class DeleteRoster(IMediator mediator) : EndpointBaseAsync
    .WithRequest<DeleteRosterCommand>
    .WithActionResult<DeleteRosterCommandResult>
{
    [HttpDelete("api/v1/rosters/{id}")]
    [SwaggerOperation(Tags = ["Rosters"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(DeleteRosterCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<DeleteRosterCommandResult>> HandleAsync(
        [FromRoute] DeleteRosterCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<DeleteRosterCommand, DeleteRosterCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

