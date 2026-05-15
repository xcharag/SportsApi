namespace SportsApi.domain.Abstractions.Live;

/// <summary>Publishes live match updates to connected SSE clients.</summary>
public interface IMatchLiveHub
{
    /// <summary>Publish a JSON payload to all active SSE subscribers for a given match.</summary>
    void Publish(Guid matchId, string json);
}
