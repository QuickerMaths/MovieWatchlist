using MovieWatchlist.Abstractions;
using MovieWatchlist.Entities;

namespace MovieWatchlist.Repositories;

public class InMemoryMovieWatchlistRepository : IWatchlistRepository<MovieWatchlistItem>
{
    private readonly Dictionary<string, MovieWatchlistItem> _items = new();

    public InMemoryMovieWatchlistRepository()
    {
        var watchListItem = new MovieWatchlistItem
        {
            Id = "watchListItemId",
            UserId = "userId",
            MovieId = 1,
            WatchStatus =  WatchStatus.WantToWatch,
            Rating = 5,
            Note = "Cool Movie",
            AddedAt = DateTime.Now
        };
        
        _items.Add(watchListItem.Id, watchListItem);
    }
    
    public async Task<IEnumerable<MovieWatchlistItem>> GetByUserAsync(string userId, WatchStatus? status = null)
    {
        var items = _items.Values
            .Where(item => item.UserId == userId && (status is null || item.WatchStatus == status))
            .ToList();

        return await Task.FromResult<IEnumerable<MovieWatchlistItem>>(items);
    }
    public async Task<MovieWatchlistItem?> GetByIdAsync(int id, string userId)
    {
        var watchListItem = _items.Values
            .Where(item => item.UserId == userId && item.MovieId == id);
        
        return await Task.FromResult(watchListItem?.FirstOrDefault());
    }

    public async Task<bool> ExistsAsync(string userId, string movieId)
    {
        var exists = _items.Values
            .Any(item => item.UserId == userId && item.Id == movieId);
        
        return await Task.FromResult(exists);
    }

    public async Task<MovieWatchlistItem> AddAsync(MovieWatchlistItem model)
    {

        if (await ExistsAsync(model.UserId, model.Id))
        {
            throw new InvalidOperationException("Watchlist item already exists");
        }
        
        _items.Add(model.Id, model);

        return model;
    }

    public async Task<Task> UpdateAsync(MovieWatchlistItem model)
    {
        if (await ExistsAsync("userId", "watchListItemId"))
        {
            throw new InvalidOperationException("Watchlist item does not exist");
        }
        
        _items[model.Id] = model;

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string movieId, string userId)
    {
        if (_items.TryGetValue(movieId, out var item) && item.UserId == userId)
        {
            _items.Remove(movieId);
        }

        return Task.CompletedTask;
    }
}