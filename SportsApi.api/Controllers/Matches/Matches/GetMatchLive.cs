using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportsApi.infrastructure.Authorization;
using SportsApi.infrastructure.Services.Live;
using Swashbuckle.AspNetCore.Annotations;
using System.Text;
using SportsApi.infrastructure.Services.Auth;

namespace SportsApi.api.Controllers.Matches.Matches;

[ApiController]
[AllowAnonymous]
[SkipDynamicPermission]
public class GetMatchLive(IMatchLiveHub liveHub) : ControllerBase
{
    [HttpGet("api/v1/matches/{matchId}/live")]
    [SwaggerOperation(
        Summary     = "Live match stream (SSE)",
        Description = "Server-Sent Events stream. Emits score, status, and event updates in real time while the match is in progress.",
        Tags        = ["Matches"])]
    public async Task HandleAsync(Guid matchId, CancellationToken cancellationToken)
    {
        Response.Headers.Append("Content-Type",  "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("X-Accel-Buffering", "no");

        var reader = liveHub.Subscribe(matchId, cancellationToken);

        try
        {
            // Send an initial heartbeat so the client knows the connection is live
            await WriteEvent("heartbeat", $"{{\"matchId\":\"{matchId}\"}}", cancellationToken);

            await foreach (var json in reader.ReadAllAsync(cancellationToken))
            {
                await WriteEvent("update", json, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // client disconnected — normal
        }
    }

    private async Task WriteEvent(string eventName, string data, CancellationToken ct)
    {
        var payload = $"event: {eventName}\ndata: {data}\n\n";
        await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(payload), ct);
        await Response.Body.FlushAsync(ct);
    }
}
