using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.TeamParticipations.Queries.GetAllTeamParticipations;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.TeamParticipations;

[ApiController]
[AllowAnonymous]
public class GetAllTeamParticipations(IMediator mediator) : EndpointBaseAsync
    .WithRequest<AllTeamParticipationsQuery>
    .WithActionResult<AllTeamParticipationsQueryResult>
{
    [HttpGet("api/v1/team-participations")]
    [SwaggerOperation(Tags = ["TeamParticipations"])]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AllTeamParticipationsQueryResult))]
    public override async Task<ActionResult<AllTeamParticipationsQueryResult>> HandleAsync(
        [FromQuery] AllTeamParticipationsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<AllTeamParticipationsQuery, AllTeamParticipationsQueryResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to retrieve team participations", detail: result.Error, statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}

