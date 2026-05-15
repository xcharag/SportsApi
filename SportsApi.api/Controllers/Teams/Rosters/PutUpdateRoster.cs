using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Rosters.Commands.PutUpdateRoster;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Rosters;

[ApiController]
[Authorize]
[DynamicPermission]
public class PutUpdateRoster(IMediator mediator) : EndpointBaseAsync
    .WithRequest<UpdateRosterCommand>
    .WithActionResult<UpdateRosterCommandResult>
{
    [HttpPut("api/v1/rosters/{id}")]
    [SwaggerOperation(Tags = ["Rosters"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(UpdateRosterCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<UpdateRosterCommandResult>> HandleAsync(
        [FromBody] UpdateRosterCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<UpdateRosterCommand, UpdateRosterCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

