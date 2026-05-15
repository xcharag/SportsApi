using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Rosters.Queries.GetAllRosters;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Rosters;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetAllRosters(IMediator mediator) : EndpointBaseAsync
    .WithRequest<AllRostersQuery>
    .WithActionResult<AllRostersQueryResult>
{
    [HttpGet("api/v1/rosters")]
    [SwaggerOperation(Tags = ["Rosters"])]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AllRostersQueryResult))]
    public override async Task<ActionResult<AllRostersQueryResult>> HandleAsync(
        [FromQuery] AllRostersQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<AllRostersQuery, AllRostersQueryResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to retrieve rosters", detail: result.Error, statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}

