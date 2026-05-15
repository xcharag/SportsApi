using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.TeamParticipations.Commands.PostCreateTeamParticipation;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.TeamParticipations;

[ApiController]
[Authorize]
[DynamicPermission]
public class PostCreateTeamParticipation(IMediator mediator) : EndpointBaseAsync
    .WithRequest<CreateTeamParticipationCommand>
    .WithActionResult<CreateTeamParticipationCommandResult>
{
    [HttpPost("api/v1/team-participations")]
    [SwaggerOperation(Tags = ["TeamParticipations"])]
    [ProducesResponseType((int)HttpStatusCode.Created,    Type = typeof(CreateTeamParticipationCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationProblemDetails))]
    public override async Task<ActionResult<CreateTeamParticipationCommandResult>> HandleAsync(
        [FromBody] CreateTeamParticipationCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<CreateTeamParticipationCommand, CreateTeamParticipationCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to create team participation", detail: result.Error, statusCode: (int)HttpStatusCode.BadRequest);

        return CreatedAtRoute("GetTeamParticipationById", new { id = result.Value.Id }, result.Value);
    }
}

