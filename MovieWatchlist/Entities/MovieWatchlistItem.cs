namespace MovieWatchlist.Entities;

public class MovieWatchlistItem
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required int MovieId { get; set; }
    public required WatchStatus WatchStatus { get; set; }
    public int? Rating { get; set; }
    public string? Note { get; set; }
    public required DateTime AddedAt { get; set; }
}