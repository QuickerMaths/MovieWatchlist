namespace MovieWatchlist.Abstractions;

public interface IMovieRepository<T>
{
    Task<IReadOnlyList<T>?> GetByUsetAsync(string userId, WatchStatus? status == null);
    Task<T?> GetByIdAsync(int id, string userId);
    Task<bool> ExistsAsync(string userId, string movieId);
    Task<T> AddAsync(T model);
    Task UpdateAsync(T model);
    Task DeleteAsync(string movieId);
}