using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Application.Services;

public class TmdbClient(HttpClient httpClient) : ITmdbClient
{
    public Task<IEnumerable<Movie>> SearchAsync(string query, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<Movie?> GetByIdAsync(int tmdbId, CancellationToken ct = default)
        => throw new NotImplementedException();
}
