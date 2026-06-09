using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Application.Abstractions;

public interface IWatchlistRepository<T>
{
    Task<IEnumerable<T>> GetByUserAsync(string userId, WatchStatus? status = null);
    Task<T?> GetByIdAsync(string id, string userId);
    Task<bool> ExistsAsync(string userId, int movieId);
    Task<T> AddAsync(T model);
    Task UpdateAsync(T model);
    Task DeleteAsync(string movieId, string userId);
}