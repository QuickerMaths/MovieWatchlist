using Microsoft.AspNetCore.Mvc;

namespace MovieWatchlist.Api.Endpoints;

public static class MovieEndpoints
{
    public static IEndpointRouteBuilder MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/movies");

        group.MapGet("/search", ([FromQuery(Name = "search")] string query) => Results.Ok);
        group.MapGet("/{id}", ([FromRoute(Name = "id")] string id) => Results.Ok());

        return app;
    }
}