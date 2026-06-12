using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Application.Contracts.MovieWatchlist;
using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Application.Services;

public class WatchlistService(
    IWatchlistRepository<MovieWatchlistItem> watchlistRepo,
    IMovieRepository<Movie> movieRepo,
    ITmdbClient tmdbClient)
{
    public Task<WatchlistItemResponse> AddItemAsync(
        string userId, AddWatchlistItemRequest request, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<IEnumerable<WatchlistItemResponse>> GetItemsAsync(
        string userId, WatchStatus? status = null, CancellationToken ct = default)
        => throw new NotImplementedException();
    
    public Task<WatchlistItemResponse?> GetItemByIdAsync(
        string id, string userId, CancellationToken ct = default)
        => throw new NotImplementedException();
    
    public Task UpdateItemAsync(
        string id, string userId, UpdateWatchlistItemRequest request, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task DeleteItemAsync(
        string id, string userId, CancellationToken ct = default)
        => throw new NotImplementedException();
}
