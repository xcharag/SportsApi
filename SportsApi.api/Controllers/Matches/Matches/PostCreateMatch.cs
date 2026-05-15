using System.Net;
using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.application.Modules.Matches.Matches.Commands.PostCreateMatch;
using SportsApi.domain.Abstractions.Messaging;
using SportsApi.infrastructure.Authorization;
using Swashbuckle.AspNetCore.Annotations;

namespace SportsApi.api.Controllers.Matches.Matches;

[ApiController]
[Authorize]
[DynamicPermission]
public class PostCreateMatch(IMediator mediator) : EndpointBaseAsync
    .WithRequest<CreateMatchCommand>
    .WithActionResult<CreateMatchCommandResult>
{
    [HttpPost("api/v1/matches")]
    [SwaggerOperation(Tags = ["Matches"])]
    [ProducesResponseType((int)HttpStatusCode.Created,    Type = typeof(CreateMatchCommandResult))]
    [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationProblemDetails))]
    public override async Task<ActionResult<CreateMatchCommandResult>> HandleAsync(
        [FromBody] CreateMatchCommand request,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.SendCommandAsync<CreateMatchCommand, CreateMatchCommandResult>(request, cancellationToken);

        if (result.IsFailure)
            return Problem(title: "Failed to create match", detail: result.Error, statusCode: (int)HttpStatusCode.BadRequest);

        return CreatedAtRoute("GetMatchById", new { id = result.Value.Id }, result.Value);
    }
}

