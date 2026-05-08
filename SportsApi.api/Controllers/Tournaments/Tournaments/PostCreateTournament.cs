using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Commands.PostCreateTournament;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[Authorize]
[DynamicPermission]
public class PostCreateTournament(IMediator mediator) : EndpointBaseAsync
    .WithRequest<CreateTournamentCommand>
    .WithActionResult<CreateTournamentCommandResult>
{
    [HttpPost("api/v1/tournaments")]
    [SwaggerOperation(Tags = new[] { "Tournaments" })]
    [ProducesResponseType((int)HttpStatusCode.Created, Type = typeof(CreateTournamentCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationProblemDetails))]
    public override async Task<ActionResult<CreateTournamentCommandResult>> HandleAsync(
        [FromBody] CreateTournamentCommand request, 
        CancellationToken cancellationToken = default)
    {
        var result = await mediator
            .SendCommandAsync<CreateTournamentCommand, CreateTournamentCommandResult>(
                request, 
                cancellationToken
                );
        
        if (result.IsFailure) return Problem(
            title: "Failed to create tournament",
            detail: result.Error,
            statusCode: (int)HttpStatusCode.BadRequest);

        return CreatedAtRoute(
            routeName: "GetTournamentById",
            routeValues: new { id = result.Value.Id },
            value: result.Value);
    }
}