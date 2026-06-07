using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Infrastructure.Repositories;

public class InMemoryMovieRepository : IMovieRepository<Movie>
{
    private readonly Dictionary<string, Movie> _movies = new();

    public InMemoryMovieRepository()
    {
        var movie = new Movie
        {
            Id = "movieId",
            TmdbId = 1,
            Overview = "Overview",
            PosterPath = "PosterPath",
            Title = "Title",
            ReleaseDate = DateTime.Now,
        };
        
        _movies.Add(movie.Id, movie);
    }
    
    public async Task<Movie?> GetByTmdbIdAsync(int tmdbId)
    {
        var movie = _movies.Values.FirstOrDefault(m => m.TmdbId == tmdbId);

        return await Task.FromResult(movie);
    }

    public Task AddAsync(Movie model)
    {
        _movies.Add(model.Id, model);

        return Task.CompletedTask;
    }
}