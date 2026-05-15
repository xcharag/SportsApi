using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Teams.Commands.PostCreateTeam;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Teams;

[ApiController]
[Authorize]
[DynamicPermission]
public class PostCreateTeam(IMediator mediator) : EndpointBaseAsync
    .WithRequest<CreateTeamCommand>
    .WithActionResult<CreateTeamCommandResult>
{
    [HttpPost("api/v1/teams")]
    [SwaggerOperation(Tags = ["Teams"])]
    [ProducesResponseType((int)HttpStatusCode.Created,    Type = typeof(CreateTeamCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationProblemDetails))]
    public override async Task<ActionResult<CreateTeamCommandResult>> HandleAsync(
        [FromBody] CreateTeamCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<CreateTeamCommand, CreateTeamCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to create team", detail: result.Error, statusCode: (int)HttpStatusCode.BadRequest);

        return CreatedAtRoute("GetTeamById", new { id = result.Value.Id }, result.Value);
    }
}

