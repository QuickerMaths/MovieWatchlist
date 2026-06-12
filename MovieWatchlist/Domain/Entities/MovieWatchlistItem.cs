namespace MovieWatchlist.Domain.Entities;

public class MovieWatchlistItem
{
    public string Id { get; set; } = string.Empty;
    public required string UserId { get; init; }
    public required int MovieId { get; init; }
    public required WatchStatus WatchStatus { get; init; }
    public int? Rating { get; set; }
    public string? Note { get; set; }
    public DateTime AddedAt { get; set; } 
}