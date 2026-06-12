using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Infrastructure.Repositories;

public class InMemoryMovieRepository : IMovieRepository<Movie>
{
    private readonly Dictionary<string, Movie> _movies = new();

    public async Task<Movie?> GetByTmdbIdAsync(int tmdbId)
    {
        var movie = _movies.Values.FirstOrDefault(m => m.TmdbId == tmdbId);

        return await Task.FromResult(movie);
    }
    public Task<bool> ExistsAsync(int tmdbId)
    {
        var exists = _movies.Values
            .Any(item => item.TmdbId == tmdbId);

        return Task.FromResult(exists);
    }

    public async Task<Movie> AddAsync(Movie model)
    {
        if (await ExistsAsync(model.TmdbId))
            throw new InvalidOperationException("Watchlist item already exists");

        if (string.IsNullOrEmpty(model.Id))
            model.Id = Guid.NewGuid().ToString();

        _movies.Add(model.Id, model);

        return model;
    }

}