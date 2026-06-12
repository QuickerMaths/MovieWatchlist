using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Application.Abstractions;

public interface ITmdbClient
{
    Task<IEnumerable<Movie>> SearchAsync(string query, CancellationToken ct = default);
    Task<Movie?> GetByIdAsync(int tmdbId, CancellationToken ct = default);
}
