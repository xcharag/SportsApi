using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.PutUpdateTeamParticipation;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.TeamParticipations;

[ApiController]
[Authorize]
[DynamicPermission]
public class PutUpdateTeamParticipation(IMediator mediator) : EndpointBaseAsync
    .WithRequest<UpdateTeamParticipationCommand>
    .WithActionResult<UpdateTeamParticipationCommandResult>
{
    [HttpPut("api/v1/team-participations/{id}")]
    [SwaggerOperation(Tags = ["TeamParticipations"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(UpdateTeamParticipationCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<UpdateTeamParticipationCommandResult>> HandleAsync(
        [FromBody] UpdateTeamParticipationCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<UpdateTeamParticipationCommand, UpdateTeamParticipationCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

