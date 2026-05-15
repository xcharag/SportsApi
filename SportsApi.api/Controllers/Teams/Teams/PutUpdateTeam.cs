using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Teams.Commands.PutUpdateTeam;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Teams;

[ApiController]
[Authorize]
[DynamicPermission]
public class PutUpdateTeam(IMediator mediator) : EndpointBaseAsync
    .WithRequest<UpdateTeamCommand>
    .WithActionResult<UpdateTeamCommandResult>
{
    [HttpPut("api/v1/teams/{id}")]
    [SwaggerOperation(Tags = ["Teams"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(UpdateTeamCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<UpdateTeamCommandResult>> HandleAsync(
        [FromBody] UpdateTeamCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<UpdateTeamCommand, UpdateTeamCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

