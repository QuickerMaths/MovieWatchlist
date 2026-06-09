namespace MovieWatchlist.Application.Contracts.Movie;

public record MovieResponse(
    int TmdbId,
    string Title,
    string? Overview,
    string? PosterPath,
    string? ReleaseDate);