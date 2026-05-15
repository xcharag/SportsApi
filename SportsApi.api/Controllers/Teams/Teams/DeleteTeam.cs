using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Teams.Commands.DeleteTeam;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Teams;

[ApiController]
[Authorize]
[DynamicPermission]
public class DeleteTeam(IMediator mediator) : EndpointBaseAsync
    .WithRequest<DeleteTeamCommand>
    .WithActionResult<DeleteTeamCommandResult>
{
    [HttpDelete("api/v1/teams/{id}")]
    [SwaggerOperation(Tags = ["Teams"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(DeleteTeamCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<DeleteTeamCommandResult>> HandleAsync(
        [FromRoute] DeleteTeamCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<DeleteTeamCommand, DeleteTeamCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

