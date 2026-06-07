using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Application.Contracts.MovieWatchlist;

public record UpdateWatchlistItemRequest(WatchStatus Status, int Rating, string Note);