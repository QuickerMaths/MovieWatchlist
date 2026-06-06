using MovieWatchlist.Entities;

namespace MovieWatchlist.Abstractions;

public interface IWatchlistRepository<T>
{
    Task<IReadOnlyList<T>?> GetByUserAsync(string userId, WatchStatus? status = null);
    Task<T?> GetByIdAsync(int id, string userId);
    Task<bool> ExistsAsync(string userId, string movieId);
    Task<T> AddAsync(T model);
    Task UpdateAsync(T model);
    Task DeleteAsync(string movieId);
}