using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Domain.Entities;
using MovieWatchlist.Infrastructure.Persistence;

namespace MovieWatchlist.Infrastructure.Repositories;

public class EfWatchlistRepository(AppDbContext db) : IWatchlistRepository<MovieWatchlistItem>
{
    public async Task<IEnumerable<MovieWatchlistItem>> GetByUserAsync(
        string userId, WatchStatus? status = null)
    {
        var query = db.WatchlistItems.AsNoTracking().Where(i => i.UserId == userId);

        if (status is not null)
            query = query.Where(i => i.WatchStatus == status);

        return await query.ToListAsync();
    }

    public Task<MovieWatchlistItem?> GetByIdAsync(string id, string userId)
        => db.WatchlistItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

    public Task<bool> ExistsAsync(string userId, int movieId)
        => db.WatchlistItems.AnyAsync(i => i.UserId == userId && i.MovieId == movieId);

    public async Task<MovieWatchlistItem> AddAsync(MovieWatchlistItem model)
    {
        if (await ExistsAsync(model.UserId, model.MovieId))
            throw new InvalidOperationException("Watchlist item already exists");

        if (string.IsNullOrEmpty(model.Id))
            model.Id = Guid.NewGuid().ToString();

        if (model.AddedAt == default)
            model.AddedAt = DateTime.UtcNow;

        db.WatchlistItems.Add(model);
        await db.SaveChangesAsync();

        return model;
    }

    public async Task UpdateAsync(MovieWatchlistItem model)
    {
        var exists = await db.WatchlistItems
            .AnyAsync(i => i.Id == model.Id && i.UserId == model.UserId);

        if (!exists)
            throw new InvalidOperationException("Watchlist item does not exist");

        db.WatchlistItems.Update(model);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id, string userId)
    {
        var item = await db.WatchlistItems
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

        if (item is null)
            return;

        db.WatchlistItems.Remove(item);
        await db.SaveChangesAsync();
    }
}
