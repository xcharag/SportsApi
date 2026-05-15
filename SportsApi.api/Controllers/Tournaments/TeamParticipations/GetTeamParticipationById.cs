using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.TeamParticipations.Queries.GetTeamParticipationById;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.TeamParticipations;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetTeamParticipationById(IMediator mediator) : EndpointBaseAsync
    .WithRequest<TeamParticipationByIdQuery>
    .WithActionResult<TeamParticipationByIdQueryResult>
{
    [HttpGet("api/v1/team-participations/{id}", Name = "GetTeamParticipationById")]
    [SwaggerOperation(Tags = ["TeamParticipations"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(TeamParticipationByIdQueryResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<TeamParticipationByIdQueryResult>> HandleAsync(
        [FromRoute] TeamParticipationByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<TeamParticipationByIdQuery, TeamParticipationByIdQueryResult>(
            new TeamParticipationByIdQuery { Id = request.Id }, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

