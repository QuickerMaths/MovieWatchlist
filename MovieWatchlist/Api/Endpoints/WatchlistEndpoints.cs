using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Application.Contracts.MovieWatchlist;

namespace MovieWatchlist.Api.Endpoints;

public static class WatchlistEndpoints
{
    public static IEndpointRouteBuilder MapWatchlistEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/watchlist").RequireAuthorization();

        group.MapGet(string.Empty,       () => Results.Ok());
        group.MapGet("/{id}",       ([FromRoute(Name = "id")] string id) => Results.Ok());
        group.MapPost(string.Empty,       ([FromBody] AddWatchlistItemRequest request) => Results.Ok());
        group.MapPut("/{id}", ([FromRoute(Name = "id")] string id, [FromBody] UpdateWatchlistItemRequest request) => Results.Ok());
        group.MapDelete("/{id}", ([FromRoute(Name = "id")] string id) => Results.NoContent());

        return app;
    }
}