using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Teams.Players.Queries.GetPlayerById;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Teams.Players;

[ApiController]
[AllowAnonymous]
public class GetPlayerById(IMediator mediator) : EndpointBaseAsync
    .WithRequest<PlayerByIdQuery>
    .WithActionResult<PlayerByIdQueryResult>
{
    [HttpGet("api/v1/players/{id}", Name = "GetPlayerById")]
    [SwaggerOperation(Tags = ["Players"])]
    [ProducesResponseType((int)HttpStatusCode.OK,       Type = typeof(PlayerByIdQueryResult))]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public override async Task<ActionResult<PlayerByIdQueryResult>> HandleAsync(
        [FromRoute] PlayerByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<PlayerByIdQuery, PlayerByIdQueryResult>(
            new PlayerByIdQuery { Id = request.Id }, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

