using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Players.Queries.GetAllPlayers;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Players;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetAllPlayers(IMediator mediator) : EndpointBaseAsync
    .WithRequest<AllPlayersQuery>
    .WithActionResult<AllPlayersQueryResult>
{
    [HttpGet("api/v1/players")]
    [SwaggerOperation(Tags = ["Players"])]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(AllPlayersQueryResult))]
    public override async Task<ActionResult<AllPlayersQueryResult>> HandleAsync(
        [FromQuery] AllPlayersQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<AllPlayersQuery, AllPlayersQueryResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to retrieve players", detail: result.Error, statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}

