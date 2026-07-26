using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Application.Contracts.MovieWatchlist;
using MovieWatchlist.Application.Services;
using MovieWatchlist.Domain.Entities;

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
        group.MapGet(string.Empty, async (
            ClaimsPrincipal user,
            WatchlistService watchlist,
            [FromQuery] WatchStatus? status,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var items = await watchlist.GetItemsAsync(userId, status, ct);
            return Results.Ok(items);
        });

        /*
         * Endpoint for WatchlistItem GET request "/watchlist/{id}"
         * Get a single watchlist item by Id
         */
        group.MapGet("/{id}", async (
            [FromRoute(Name = "id")] string id,
            ClaimsPrincipal user,
            WatchlistService watchlist,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var item = await watchlist.GetItemByIdAsync(id, userId, ct);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });
        
        /*
         * Endpoint for WatchlistItem POST request "/watchlist"
         * Add a movie to the watchlist
         */
        group.MapPost(string.Empty, async (
            [FromBody] AddWatchlistItemRequest request,
            ClaimsPrincipal user,
            WatchlistService watchlist,
            IWatchlistRepository<MovieWatchlistItem> watchlistRepo,
            CancellationToken ct) =>
        {
            if (request.TmbdId <= 0)
                return Results.BadRequest();

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (await watchlistRepo.ExistsAsync(userId, request.TmbdId))
                return Results.Conflict();

            var item = await watchlist.AddItemAsync(userId, request, ct);
            return Results.Created($"/watchlist/{item.Id}", item);
        });

        /*
         * Endpoint for WatchlistItem PUT request "/watchlist/{id}"
         * Update status / rating / note
         */
        group.MapPut("/{id}", async (
            [FromRoute(Name = "id")] string id,
            [FromBody] UpdateWatchlistItemRequest request,
            ClaimsPrincipal user,
            WatchlistService watchlist,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            try
            {
                await watchlist.UpdateItemAsync(id, userId, request, ct);
                return Results.NoContent();
            }
            catch (ArgumentOutOfRangeException)
            {
                return Results.BadRequest();
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        /*
         * Endpoint for WatchlistItem DELETE request "/watchlist/{id}"
         * Remove an item
         */
        group.MapDelete("/{id}", async (
            [FromRoute(Name = "id")] string id,
            ClaimsPrincipal user,
            WatchlistService watchlist,
            CancellationToken ct) =>
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await watchlist.DeleteItemAsync(id, userId, ct);
            return Results.NoContent();
        });

        return app;
    }
}