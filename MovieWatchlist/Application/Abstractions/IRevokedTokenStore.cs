namespace MovieWatchlist.Application.Abstractions;

public interface IRevokedTokenStore
{
    Task RevokeAsync(string jti, DateTime expiresUtc, CancellationToken ct = default);
    Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default);
}
