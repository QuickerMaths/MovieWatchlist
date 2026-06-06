namespace MovieWatchlist.Abstractions;

public interface IWatchlistRepository<T>
{
    Task<T?> GetByTmdbIdAsync(int  tmdbId);
    Task<T> AddAsync(T model);
}