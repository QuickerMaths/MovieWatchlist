using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Application.Contracts.Movie;
using MovieWatchlist.Domain.Entities;

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
        group.MapGet("/search", async (
            [FromQuery] string? query,
            ITmdbClient tmdbClient,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(query))
                return Results.BadRequest();

            var movies = await tmdbClient.SearchAsync(query, ct);
            return Results.Ok(movies.Select(ToResponse));
        });

        /*
         * Endpoint for Movie GET request "/movies/{tmdbId}"
         * Get details for one movie, from the local store or TMDB
         */
        group.MapGet("/{id}", async (
            [FromRoute] string id,
            IMovieRepository<Movie> movieRepo,
            ITmdbClient tmdbClient,
            CancellationToken ct) =>
        {
            if (!int.TryParse(id, out var tmdbId))
                return Results.BadRequest();

            var movie = await movieRepo.GetByTmdbIdAsync(tmdbId)
                        ?? await tmdbClient.GetByIdAsync(tmdbId, ct);

            return movie is null ? Results.NotFound() : Results.Ok(ToResponse(movie));
        });

        return app;
    }

    private static MovieResponse ToResponse(Movie movie) =>
        new(movie.TmdbId, movie.Title, movie.Overview, movie.PosterPath,
            movie.ReleaseDate.ToString("yyyy-MM-dd"));
}
