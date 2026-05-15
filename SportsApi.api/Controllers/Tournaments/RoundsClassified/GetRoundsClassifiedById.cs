using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.RoundsClassified.Queries.GetRoundsClassifiedById;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.RoundsClassified;

[ApiController]
[AllowAnonymous]
public class GetRoundsClassifiedById(IMediator mediator) : EndpointBaseAsync
    .WithRequest<RoundsClassifiedByIdQuery>
    .WithActionResult<RoundsClassifiedByIdQueryResult>
{
    [HttpGet("api/v1/rounds-classified/{id}", Name = "GetRoundsClassifiedById")]
    [SwaggerOperation(Tags = ["RoundsClassified"])]
    [ProducesResponseType(StatusCodes.Status200OK,  Type = typeof(RoundsClassifiedByIdQueryResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public override async Task<ActionResult<RoundsClassifiedByIdQueryResult>> HandleAsync(
        [FromRoute] RoundsClassifiedByIdQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<RoundsClassifiedByIdQuery, RoundsClassifiedByIdQueryResult>(
            request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}

