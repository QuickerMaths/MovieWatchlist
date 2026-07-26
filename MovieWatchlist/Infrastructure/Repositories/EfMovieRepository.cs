using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Domain.Entities;
using MovieWatchlist.Infrastructure.Persistence;

namespace MovieWatchlist.Infrastructure.Repositories;

public class EfMovieRepository(AppDbContext db) : IMovieRepository<Movie>
{
    public Task<Movie?> GetByTmdbIdAsync(int tmdbId)
        => db.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.TmdbId == tmdbId);

    public Task<bool> ExistsAsync(int tmdbId)
        => db.Movies.AnyAsync(m => m.TmdbId == tmdbId);

    public async Task<Movie> AddAsync(Movie model)
    {
        if (await ExistsAsync(model.TmdbId))
            throw new InvalidOperationException("Movie already exists");

        if (string.IsNullOrEmpty(model.Id))
            model.Id = Guid.NewGuid().ToString();

        db.Movies.Add(model);
        await db.SaveChangesAsync();

        return model;
    }
}
