using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Tournaments.Tournaments.Commands.PutUpdateTournament;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Tournaments.Tournaments;

[ApiController]
[Authorize]
[DynamicPermission]
public class PutUpdateTournament(IMediator mediator) : EndpointBaseAsync
    .WithRequest<UpdateTournamentCommand>
    .WithActionResult<UpdateTournamentCommandResult>
{
    [HttpPut("api/tournaments/{id}")]
    [SwaggerOperation(Tags = ["Tournaments"])]
    [ProducesResponseType((int)System.Net.HttpStatusCode.OK, Type = typeof(UpdateTournamentCommandResult))]
    [ProducesResponseType((int)System.Net.HttpStatusCode.NotFound)]
    public override async Task<ActionResult<UpdateTournamentCommandResult>> HandleAsync(
        [FromBody] UpdateTournamentCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<UpdateTournamentCommand, UpdateTournamentCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}