namespace MovieWatchlist.Domain.Entities;

public class Movie
{
    public string Id { get; set; }
    public required int TmdbId { get; init; }
    public required string Title  { get; init; }
    public required string Overview  { get; init; }
    public required string PosterPath { get; init; }
    public required DateTime ReleaseDate { get; init; }
}