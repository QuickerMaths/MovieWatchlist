using MovieWatchlist.Abstractions;
using MovieWatchlist.Entities;

namespace MovieWatchlist.Repositories;

public class InMemoryMovieRepository : IMovieRepository<Movie>
{
    public Task<Movie?> GetByTmdbIdAsync(int tmdbId)
    {
        throw new NotImplementedException();
    }

    public Task<Movie> AddAsync(Movie model)
    {
        throw new NotImplementedException();
    }
}