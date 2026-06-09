using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Application.Contracts.Movie;
using MovieWatchlist.Domain.Entities;

namespace Tests.Api;

public class MovieEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;
    
    private async Task SeedAsync(params Movie[] items)
    {
        var repo = _factory.Services.GetRequiredService<IMovieRepository<Movie>>();

        foreach (var item in items)
            await repo.AddAsync(item);
    }


    /*
    * Tests for Movie GET request "/movies/search?query="
    * Search TMDB for movies by title
    */
    
    [Fact]
    public async Task SearchMovies_Returns_200WithListOfQueriedMovies()
    {
        const string queryTitle = "inception";
        var client = _factory.CreateClient();
        
        var response = await client.GetAsync($"/movies/search?{queryTitle}", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<MovieResponse>>(TestContext.Current.CancellationToken);
        
        Assert.NotNull(items);
        Assert.NotEmpty(items);
    }
    
        
    [Fact]
    public async Task SearchMovies_Returns400_IfTheQueryIsMissingOrEmpty()
    {
        string query = null!;
        var client = _factory.CreateClient();
        
        var response = await client.GetAsync($"/movies?search={query}", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    /*
     * Tests for Movie GET request "/movies/{tmdbId}"
     * Get details for one movie from TMDB
     */

    [Fact]
    public async Task GetMovieByTmdbId_Returns200_WithSingleMovieRecord()
    {
        const int movieId = 1;
        var client = _factory.CreateClient();
        
        var response = await client.GetAsync($"/movies/{movieId}", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var items = await response.Content.ReadFromJsonAsync<List<MovieResponse>>(TestContext.Current.CancellationToken);

        Assert.NotNull(items);
        Assert.Single(items);
        Assert.Contains(items, i => i.TmdbId == movieId);
    }
    
    [Fact]
    public async Task GetMovieByTmdbId_Returns400_WhenProvidedStringInsteadOfInt()
    {
        const string stringId = "stringId";
        
        var client = _factory.CreateClient();
        
        var response = await client.GetAsync($"/movies/{stringId}", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task GetMovieByTmdbId_Returns404_WhenTheMovieWasNotFount()
    {
        const int movieId = 1;
        const int notFoundMovieId = 2;
        await SeedAsync(new Movie
        {
            TmdbId = movieId,
            Title = "inception",
            Overview = "inception movie",
            ReleaseDate = DateTime.UtcNow,
            PosterPath = "poster-path"
        });
        var client = _factory.CreateClient();
        
        var response = await client.GetAsync($"/movies/{notFoundMovieId}", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}