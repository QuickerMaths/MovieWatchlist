namespace MovieWatchlist.Entities;

public class Movie
{
    public required string Id { get; set; };
    public required int TmdbId { get; set; };
    public required string Title  { get; set; };
    public required string Overview  { get; set; };
    public string? PosterPath { get; set; };
    public required DateTime ReleaseDate { get; set; };
}