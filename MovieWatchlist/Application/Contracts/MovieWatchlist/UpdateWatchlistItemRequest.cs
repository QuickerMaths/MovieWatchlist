using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Application.Contracts.MovieWatchlist;

public record UpdateWatchlistItemRequest(WatchStatus? WatchStatus, int? Rating, string? Note);