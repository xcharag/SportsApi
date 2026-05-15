using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.RoundsClassified.Queries.GetAllRoundsClassified;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.RoundsClassified;

[ApiController]
[Authorize]
[DynamicPermission]
public class GetAllRoundsClassified(IMediator mediator) : EndpointBaseAsync
    .WithRequest<AllRoundsClassifiedQuery>
    .WithActionResult<AllRoundsClassifiedQueryResult>
{
    [HttpGet("api/v1/rounds-classified")]
    [SwaggerOperation(
        Summary = "List rounds-classified entries",
        Description = "Returns paginated RoundsClassified records. Active = team is still competing in the tournament at that round.",
        Tags = ["RoundsClassified"])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AllRoundsClassifiedQueryResult))]
    public override async Task<ActionResult<AllRoundsClassifiedQueryResult>> HandleAsync(
        [FromQuery] AllRoundsClassifiedQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendQueryAsync<AllRoundsClassifiedQuery, AllRoundsClassifiedQueryResult>(
            request, cancellationToken);

        if (result.IsFailure)
            return Problem(
                title:      "Failed to retrieve rounds-classified",
                detail:     result.Error,
                statusCode: StatusCodes.Status500InternalServerError);

        return Ok(result.Value);
    }
}

