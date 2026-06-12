namespace MovieWatchlist.Application.Abstractions;

public interface IMovieRepository<T>
{
    Task<T?> GetByTmdbIdAsync(int  tmdbId); 
    Task<bool> ExistsAsync(int tmdbId);
    Task<T> AddAsync(T model);
}