using System.Collections.Concurrent;
using System.Threading.Channels;
using DomainHub = SportsApi.domain.Abstractions.Live;

namespace SportsApi.infrastructure.Services.Live;

/// <summary>
/// Provides per-match SSE broadcast channels so that command handlers can publish
/// live updates and the SSE controller can stream them to clients.
/// </summary>
public interface IMatchLiveHub : DomainHub.IMatchLiveHub
{
    /// <summary>Subscribe to live events for a match. Returns a reader that will receive published messages.</summary>
    ChannelReader<string> Subscribe(Guid matchId, CancellationToken cancellationToken);

    /// <summary>Remove a subscriber channel when a client disconnects.</summary>
    void Unsubscribe(Guid matchId, Channel<string> channel);
}

public sealed class MatchLiveHub : IMatchLiveHub
{
    // matchId → list of unbounded channels (one per connected SSE client)
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<Channel<string>>> _subscribers = new();

    public void Publish(Guid matchId, string json)
    {
        if (!_subscribers.TryGetValue(matchId, out var channels)) return;

        foreach (var channel in channels)
            channel.Writer.TryWrite(json);
    }

    public ChannelReader<string> Subscribe(Guid matchId, CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<string>(
            new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

        var bag = _subscribers.GetOrAdd(matchId, _ => new ConcurrentBag<Channel<string>>());
        bag.Add(channel);

        // Complete the writer when the client disconnects
        cancellationToken.Register(() =>
        {
            channel.Writer.TryComplete();
            Unsubscribe(matchId, channel);
        });

        return channel.Reader;
    }

    public void Unsubscribe(Guid matchId, Channel<string> channel)
    {
        if (!_subscribers.TryGetValue(matchId, out var bag)) return;

        var updated = new ConcurrentBag<Channel<string>>(bag.Where(c => !ReferenceEquals(c, channel)));
        _subscribers.TryUpdate(matchId, updated, bag);
    }
}
