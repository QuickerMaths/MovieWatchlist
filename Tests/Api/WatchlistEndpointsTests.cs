using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MovieWatchlist.Application.Contracts.MovieWatchlist;
using MovieWatchlist.Domain.Entities;

namespace Tests.Api;

public class WatchlistEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory = factory;

    /*
     * Tests for WatchlistItem GET request "/watchlist"
     * Lists the current user's items
     */

    [Fact]
    public async Task GetWatchlistItem_Returns401_WhenUnauthorized()
    {
        var client = _factory.CreateClient();
        
        var response = await client.GetAsync("/watchlist", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    /*
     * Tests for WatchlistItem GET request "/watchlist/{id}"
     * Get a single watchlist item by Id
     */

    [Fact]
    public async Task GetWatchlistItemById_Returns401_WhenUnauthorized()
    {
        var client = _factory.CreateClient();
        
        var response = await client.GetAsync("/watchlist/1", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    /*
     * Tests for WatchlistItem POST request "/watchlist"
     * Add a movie to the watchlist
     */
    
    [Fact]
    public async Task AddWatchlistItem_Returns401_WhenUnauthorized()
    {
        var client = _factory.CreateClient();

        var requestBody = new AddWatchlistItemRequest(1, WatchStatus.WantToWatch);
        
        var response = await client.PostAsJsonAsync("/watchlist", requestBody, cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /*
     * Tests for WatchlistItem PUT request "/watchlist/{id}"
     * Update status / rating / note
     */
    
    [Fact]
    public async Task UpdateWatchlistItem_Returns401_WhenUnauthorized()
    {
        var client = _factory.CreateClient();

        var requestBody = new UpdateWatchlistItemRequest(WatchStatus.Watching, 4, "Note");
        
        var response = await client.PutAsJsonAsync("/watchlist/1", requestBody, cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    /*
     * Tests for WatchlistItem DELETE request "/watchlist/{id}"
     * Remove an item
     */

    [Fact]
    public async Task DeleteWatchlistItem_Returns401_WhenUnauthorized()
    {
        var client = _factory.CreateClient();
        
        var response = await client.DeleteAsync($"watchlist/1", cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}