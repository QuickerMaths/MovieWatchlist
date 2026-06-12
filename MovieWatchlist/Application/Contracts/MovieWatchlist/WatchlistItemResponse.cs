using MovieWatchlist.Application.Contracts.Movie;
using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Application.Contracts.MovieWatchlist;

public record WatchlistItemResponse(
    string Id,
    WatchStatus WatchStatus,
    int? Rating,
    string? Note,
    DateTime AddedAt,
    DateTime? WatchedAt,
    MovieResponse Movie);