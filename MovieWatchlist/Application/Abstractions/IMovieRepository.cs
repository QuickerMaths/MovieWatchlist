namespace MovieWatchlist.Application.Abstractions;

public interface IMovieRepository<T>
{
    Task<T?> GetByTmdbIdAsync(int  tmdbId);
    Task AddAsync(T model);
}