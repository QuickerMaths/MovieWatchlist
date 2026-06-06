namespace MovieWatchlist.Abstractions;

public interface IMovieRepository<T>
{
    Task<T?> GetByTmdbIdAsync(int  tmdbId);
    Task<T> AddAsync(T model);
}