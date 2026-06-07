using Microsoft.AspNetCore.Mvc;

namespace MovieWatchlist.Api.Endpoints;

public static class WatchlistEndpoints
{
    public static IEndpointRouteBuilder MapWatchlistEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/watchlist");

        group.MapGet(string.Empty,       () => Results.Ok());
        group.MapGet("/{id}",       ([FromRoute(Name = "id")] string id) => Results.Ok());
        group.MapPost(string.Empty,       () => Results.Ok());
        group.MapPut("/{id}", ([FromRoute(Name = "id")] string id) => Results.Ok());
        group.MapDelete("/{id}", ([FromRoute(Name = "id")] string id) => Results.NoContent());

        return app;
    }
}