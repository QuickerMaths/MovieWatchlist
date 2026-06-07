using Microsoft.AspNetCore.Mvc;

namespace MovieWatchlist.Api.Endpoints;

public static class MovieEndpoints
{
    public static IEndpointRouteBuilder MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/movies");

        /*
         * Endpoint for Movie GET request "/movies/search?query="
         * Search TMDB for movies by title
         */
        group.MapGet("/search", ([FromQuery(Name = "search")] string query) => Results.Ok);
        
        /*
         * Endpoint for Movie GET request "/movies/{tmdbId}"
         * Get details for one movie from TMDB
         */
        group.MapGet("/{id}", ([FromRoute(Name = "id")] string id) => Results.Ok());

        return app;
    }
}