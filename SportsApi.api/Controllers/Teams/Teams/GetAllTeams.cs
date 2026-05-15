using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Teams.Queries.GetAllTeams;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Teams;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetAllTeams(IMediator mediator) : EndpointBaseAsync
    .WithRequest<AllTeamsQuery>
    .WithActionResult<AllTeamsQueryResult>
{
    [HttpGet("api/v1/teams")]
    [SwaggerOperation(Tags = ["Teams"])]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AllTeamsQueryResult))]
    public override async Task<ActionResult<AllTeamsQueryResult>> HandleAsync(
        [FromQuery] AllTeamsQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<AllTeamsQuery, AllTeamsQueryResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to retrieve teams", detail: result.Error, statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}

