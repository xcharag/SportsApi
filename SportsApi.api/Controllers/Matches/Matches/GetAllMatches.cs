using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Matches.Matches.Queries.GetAllMatches;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Matches.Matches;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetAllMatches(IMediator mediator) : EndpointBaseAsync
    .WithRequest<AllMatchesQuery>
    .WithActionResult<AllMatchesQueryResult>
{
    [HttpGet("api/v1/matches")]
    [SwaggerOperation(Tags = ["Matches"])]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AllMatchesQueryResult))]
    public override async Task<ActionResult<AllMatchesQueryResult>> HandleAsync(
        [FromQuery] AllMatchesQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<AllMatchesQuery, AllMatchesQueryResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to retrieve matches", detail: result.Error, statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}

