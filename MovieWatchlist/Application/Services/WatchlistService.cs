using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Application.Contracts.Movie;
using MovieWatchlist.Application.Contracts.MovieWatchlist;
using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Application.Services;

public class WatchlistService(
    IWatchlistRepository<MovieWatchlistItem> watchlistRepo,
    IMovieRepository<Movie> movieRepo,
    ITmdbClient tmdbClient)
{
    public async Task<WatchlistItemResponse> AddItemAsync(
        string userId, AddWatchlistItemRequest request, CancellationToken ct = default)
    {
        var movie = await movieRepo.GetByTmdbIdAsync(request.TmbdId);
        if (movie is null)
        {
            movie = await tmdbClient.GetByIdAsync(request.TmbdId, ct)
                ?? throw new InvalidOperationException($"Movie {request.TmbdId} not found on TMDB");
            await movieRepo.AddAsync(movie);
        }

        var item = await watchlistRepo.AddAsync(new MovieWatchlistItem
        {
            UserId = userId,
            MovieId = request.TmbdId,
            WatchStatus = request.Status
        });

        return ToResponse(item, movie);
    }

    public async Task<IEnumerable<WatchlistItemResponse>> GetItemsAsync(
        string userId, WatchStatus? status = null, CancellationToken ct = default)
    {
        var items = await watchlistRepo.GetByUserAsync(userId, status);

        var responses = new List<WatchlistItemResponse>();
        foreach (var item in items)
        {
            var movie = await movieRepo.GetByTmdbIdAsync(item.MovieId);
            responses.Add(ToResponse(item, movie));
        }

        return responses;
    }

    public async Task<WatchlistItemResponse?> GetItemByIdAsync(
        string id, string userId, CancellationToken ct = default)
    {
        var item = await watchlistRepo.GetByIdAsync(id, userId);
        if (item is null)
            return null;

        var movie = await movieRepo.GetByTmdbIdAsync(item.MovieId);
        return ToResponse(item, movie);
    }

    public async Task UpdateItemAsync(
        string id, string userId, UpdateWatchlistItemRequest request, CancellationToken ct = default)
    {
        if (request.Rating is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(request), request.Rating, "Rating must be 1–10.");

        var item = await watchlistRepo.GetByIdAsync(id, userId);
        if (item is null)
            throw new InvalidOperationException("Watchlist item does not exist");

        var updated = new MovieWatchlistItem
        {
            Id = item.Id,
            UserId = item.UserId,
            MovieId = item.MovieId,
            WatchStatus = request.WatchStatus ?? item.WatchStatus,
            Rating = request.Rating,
            Note = request.Note,
            AddedAt = item.AddedAt
        };

        await watchlistRepo.UpdateAsync(updated);
    }

    public Task DeleteItemAsync(string id, string userId, CancellationToken ct = default)
        => watchlistRepo.DeleteAsync(id, userId);

    private static WatchlistItemResponse ToResponse(MovieWatchlistItem item, Movie? movie) =>
        new(
            Id: item.Id,
            WatchStatus: item.WatchStatus,
            Rating: item.Rating,
            Note: item.Note,
            AddedAt: item.AddedAt,
            WatchedAt: null,
            Movie: new MovieResponse(
                TmdbId: movie?.TmdbId ?? item.MovieId,
                Title: movie?.Title ?? string.Empty,
                Overview: movie?.Overview,
                PosterPath: movie?.PosterPath,
                ReleaseDate: movie?.ReleaseDate.ToString("yyyy-MM-dd")));
}
