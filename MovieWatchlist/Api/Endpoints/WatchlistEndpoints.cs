using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Application.Contracts.MovieWatchlist;

namespace MovieWatchlist.Api.Endpoints;

public static class WatchlistEndpoints
{
    public static IEndpointRouteBuilder MapWatchlistEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/watchlist").RequireAuthorization();

        /*
         * Endpoint for WatchlistItem GET request "/watchlist"
         * Lists the current user's items
         */
        group.MapGet(string.Empty,       () => Results.Ok());
        
        /*
         * Endpoint for WatchlistItem GET request "/watchlist/{id}"
         * Get a single watchlist item by Id
         */
        group.MapGet("/{id}",       ([FromRoute(Name = "id")] string id) => Results.Ok());
        
        /*
         * Endpoint for WatchlistItem POST request "/watchlist"
         * Add a movie to the watchlist
         */
        group.MapPost(string.Empty,       ([FromBody] AddWatchlistItemRequest request) => Results.Ok());
        
        /*
         * Endpoint for WatchlistItem PUT request "/watchlist/{id}"
         * Update status / rating / note
         */
        group.MapPut("/{id}", ([FromRoute(Name = "id")] string id, [FromBody] UpdateWatchlistItemRequest request) => Results.Ok());
        
        /*
         * Endpoint for WatchlistItem DELETE request "/watchlist/{id}"
         * Remove an item
         */
        group.MapDelete("/{id}", ([FromRoute(Name = "id")] string id) => Results.NoContent());

        return app;
    }
}