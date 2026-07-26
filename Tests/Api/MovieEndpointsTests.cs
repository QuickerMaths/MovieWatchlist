using System.Net;
using System.Net.Http.Json;
using MovieWatchlist.Application.Contracts.Movie;
using MovieWatchlist.Domain.Entities;
using NSubstitute;
using Tests.Helpers;

namespace Tests.Api;

public class MovieEndpointsTests(RealAuthWebAppFactory factory) : IClassFixture<RealAuthWebAppFactory>
{
    private readonly RealAuthWebAppFactory _factory = factory;

    private static Movie Sample(int tmdbId) => new()
    {
        TmdbId = tmdbId,
        Title = "inception",
        Overview = "inception movie",
        PosterPath = "poster-path",
        ReleaseDate = new DateTime(2010, 7, 16)
    };

    /*
     * Tests for Movie GET request "/movies/search?query="
     * Search TMDB for movies by title
     */

    [Fact]
    public async Task SearchMovies_Returns200WithListOfQueriedMovies()
    {
        const string query = "inception";
        _factory.TmdbClient.SearchAsync(query, Arg.Any<CancellationToken>())
            .Returns(new[] { Sample(1) });
        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            $"/movies/search?query={query}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<MovieResponse>>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(items);
        Assert.NotEmpty(items);
    }

    [Fact]
    public async Task SearchMovies_Returns400_IfTheQueryIsMissingOrEmpty()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            "/movies/search?query=", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /*
     * Tests for Movie GET request "/movies/{tmdbId}"
     * Get details for one movie
     */

    [Fact]
    public async Task GetMovieByTmdbId_Returns200WithSingleMovieRecord()
    {
        const int movieId = 1;
        _factory.TmdbClient.GetByIdAsync(movieId, Arg.Any<CancellationToken>())
            .Returns(Sample(movieId));
        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            $"/movies/{movieId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var item = await response.Content.ReadFromJsonAsync<MovieResponse>(
            TestContext.Current.CancellationToken);

        Assert.NotNull(item);
        Assert.Equal(movieId, item.TmdbId);
    }

    [Fact]
    public async Task GetMovieByTmdbId_Returns400_WhenProvidedStringInsteadOfInt()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            "/movies/stringId", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMovieByTmdbId_Returns404_WhenTheMovieWasNotFound()
    {
        const int notFoundMovieId = 404404; // unarranged substitute returns null
        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            $"/movies/{notFoundMovieId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
