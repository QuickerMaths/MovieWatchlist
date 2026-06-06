using MovieWatchlist.Abstractions;
using MovieWatchlist.Entities;

namespace MovieWatchlist.Repositories;

public class InMemoryMovieWatchlistRepository : IWatchlistRepository<MovieWatchlistItem>
{
    public Task<IReadOnlyList<MovieWatchlistItem>?> GetByUserAsync(string userId, WatchStatus? status = null)
    {
        throw new NotImplementedException();
    }

    public Task<MovieWatchlistItem?> GetByIdAsync(int id, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(string userId, string movieId)
    {
        throw new NotImplementedException();
    }

    public Task<MovieWatchlistItem> AddAsync(MovieWatchlistItem model)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(MovieWatchlistItem model)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string movieId)
    {
        throw new NotImplementedException();
    }
}