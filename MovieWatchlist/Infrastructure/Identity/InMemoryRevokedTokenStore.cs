using System.Collections.Concurrent;
using MovieWatchlist.Application.Abstractions;

namespace MovieWatchlist.Infrastructure.Identity;

public class InMemoryRevokedTokenStore : IRevokedTokenStore
{
    // TODO: no expiry cleanup; entries live until process restart. Swap for a DB/cache with TTL later.
    private readonly ConcurrentDictionary<string, DateTime> _revoked = new();

    public Task RevokeAsync(string jti, DateTime expiresUtc, CancellationToken ct = default)
    {
        _revoked[jti] = expiresUtc;
        return Task.CompletedTask;
    }

    public Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default)
        => Task.FromResult(_revoked.ContainsKey(jti));
}
