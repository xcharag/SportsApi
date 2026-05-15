using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.DeleteTeamParticipation;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.TeamParticipations;

[ApiController]
[Authorize]
[DynamicPermission]
public class DeleteTeamParticipation(IMediator mediator) : EndpointBaseAsync
    .WithRequest<DeleteTeamParticipationCommand>
    .WithActionResult<DeleteTeamParticipationCommandResult>
{
    [HttpDelete("api/v1/team-participations/{id}")]
    [SwaggerOperation(Tags = ["TeamParticipations"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(DeleteTeamParticipationCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<DeleteTeamParticipationCommandResult>> HandleAsync(
        [FromRoute] DeleteTeamParticipationCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<DeleteTeamParticipationCommand, DeleteTeamParticipationCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

