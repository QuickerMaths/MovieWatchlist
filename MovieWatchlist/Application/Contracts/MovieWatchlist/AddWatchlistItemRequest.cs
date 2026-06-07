using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Application.Contracts.MovieWatchlist;

public record AddWatchlistItemRequest(int TmbdId, WatchStatus Status);