using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Infrastructure.Repositories;

public class InMemoryMovieWatchlistRepository : IWatchlistRepository<MovieWatchlistItem>
{
    private readonly Dictionary<string, MovieWatchlistItem> _items = new();

    public Task<IEnumerable<MovieWatchlistItem>> GetByUserAsync(
        string userId, WatchStatus? status = null)
    {
        var items = _items.Values
            .Where(item => item.UserId == userId
                           && (status is null || item.WatchStatus == status))
            .ToList();

        return Task.FromResult<IEnumerable<MovieWatchlistItem>>(items);
    }

    public Task<MovieWatchlistItem?> GetByIdAsync(string id, string userId)
    {
        var item = _items.TryGetValue(id, out var found) && found.UserId == userId
            ? found
            : null;

        return Task.FromResult(item);
    }

    public Task<bool> ExistsAsync(string userId, int movieId)
    {
        var exists = _items.Values
            .Any(item => item.UserId == userId && item.MovieId == movieId);

        return Task.FromResult(exists);
    }

    public async Task<MovieWatchlistItem> AddAsync(MovieWatchlistItem model)
    {
        if (await ExistsAsync(model.UserId, model.MovieId))
            throw new InvalidOperationException("Watchlist item already exists");

        if (string.IsNullOrEmpty(model.Id))
            model.Id = Guid.NewGuid().ToString();

        if (model.AddedAt == default)
            model.AddedAt = DateTime.UtcNow;

        _items.Add(model.Id, model);

        return model;
    }

    public Task UpdateAsync(MovieWatchlistItem model)
    {
        if (!_items.TryGetValue(model.Id, out var existing) || existing.UserId != model.UserId)
            throw new InvalidOperationException("Watchlist item does not exist");

        _items[model.Id] = model;

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, string userId)
    {
        if (_items.TryGetValue(id, out var item) && item.UserId == userId)
            _items.Remove(id);

        return Task.CompletedTask;
    }

    public Task Clear()
    {
        _items.Clear();
        return Task.CompletedTask;
    }
}