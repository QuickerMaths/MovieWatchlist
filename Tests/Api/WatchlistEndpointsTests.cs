using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MovieWatchlist.Application.Contracts.MovieWatchlist;
using MovieWatchlist.Domain.Entities;
using MovieWatchlist.Infrastructure.Repositories;
using Tests.Helpers;

namespace Tests.Api;

public class WatchlistEndpointTests(
    TestWebAppFactory authFactory,
    WebApplicationFactory<Program> plainFactory)
    :
        IClassFixture<TestWebAppFactory>,
        IClassFixture<WebApplicationFactory<Program>>,
        IDisposable
{
    private readonly TestWebAppFactory _authFactory = authFactory;
    private readonly WebApplicationFactory<Program> _factory = plainFactory;
    
    public void Dispose()
    {
        var repo = _authFactory.Services.GetRequiredService<InMemoryMovieWatchlistRepository>();

        repo.Clear();
    }

    private async Task SeedAsync(params MovieWatchlistItem[] items)
    {
        var repo = _authFactory.Services.GetRequiredService<InMemoryMovieWatchlistRepository>();

        foreach (var item in items)
            await repo.AddAsync(item);
    }


    /*
     * Tests for WatchlistItem GET request "/watchlist"
     * Lists the current user's items
     */

    [Fact]
    public async Task GetWatchlistItem_Returns200WithAnEmptyList_WhenAuthenticated()
    {
        var client = _authFactory.CreateClient();
        
        var response = await client.GetAsync("/watchlist", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<WatchlistItemResponse>>(TestContext.Current.CancellationToken);
        
        Assert.NotNull(items);
        Assert.Empty(items);
    }
    
    [Fact]
    public async Task GetWatchlistItem_Returns200WithAnListOfItems_WhenAuthenticated()
    {
        await SeedAsync(new MovieWatchlistItem
        {
            UserId = TestAuthHandler.TestUserId,
            MovieId = 1,
            WatchStatus = WatchStatus.WantToWatch
        },
        new MovieWatchlistItem
        {
            UserId = TestAuthHandler.TestUserId,
            MovieId = 2,
            WatchStatus = WatchStatus.Finished
        },
        new MovieWatchlistItem
        {
            Id = "some-other-user-item-id",
            UserId = "some-other-user",
            MovieId = 2,
            WatchStatus = WatchStatus.Finished
        });
        var client = _authFactory.CreateClient();
        
        var response = await client.GetAsync("/watchlist", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<WatchlistItemResponse>>(TestContext.Current.CancellationToken);
        
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
        Assert.Contains(items, i => i.WatchStatus == WatchStatus.WantToWatch);
        Assert.Contains(items, i => i.WatchStatus == WatchStatus.Finished);
        Assert.DoesNotContain(items, i => i.Id == "some-other-user-item-id");
    }
    
    [Fact]
    public async Task GetWatchlistItem_Returns200WithAnListOfItemsAndStatusFinished_WhenAuthenticated()
    {
        await SeedAsync(new MovieWatchlistItem
        {
            UserId = TestAuthHandler.TestUserId,
            MovieId = 1,
            WatchStatus = WatchStatus.WantToWatch,
        },new MovieWatchlistItem
        {
            UserId = TestAuthHandler.TestUserId,
            MovieId = 2,
            WatchStatus = WatchStatus.Finished,
        },new MovieWatchlistItem
        {
            UserId = TestAuthHandler.TestUserId,
            MovieId = 3,
            WatchStatus = WatchStatus.Finished,
        });
        var client = _authFactory.CreateClient();
        
        var response = await client.GetAsync("/watchlist?status=Finished", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<WatchlistItemResponse>>(TestContext.Current.CancellationToken);
        
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
        Assert.All(items, i => Assert.Equal(WatchStatus.Finished, i.WatchStatus));
    }

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
    public async Task GetWatchlistItemById_Returns200AndSingleItem_WhenAuthorized()
    {
        const int movieId = 1;
        await SeedAsync(new MovieWatchlistItem
        {
            UserId = TestAuthHandler.TestUserId,
            MovieId = movieId,
            WatchStatus = WatchStatus.WantToWatch
        });
        
        var client = _authFactory.CreateClient();
        
        var response = await client.GetAsync($"watchlist/1", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var items = await response.Content.ReadFromJsonAsync<List<WatchlistItemResponse>>(TestContext.Current.CancellationToken);
        
        Assert.NotNull(items);
        Assert.Single(items);
        Assert.Equal(WatchStatus.WantToWatch, items[0].WatchStatus);
    }

    [Fact]
    public async Task GetWatchlistItemById_Returns404WhenItemIsNotFound_WhenAuthorized()
    {
        var client = _authFactory.CreateClient();
        
        var response = await client.GetAsync($"watchlist/1", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task? GetWatchlistItemById_Returns404WhenTheItemIsNotOwnedByTheUser_WhenAuthorized()
    {
        await SeedAsync(new MovieWatchlistItem
        {
            Id = "other-users-item",
            UserId = "otherUsersId",
            MovieId = 1,
            WatchStatus = WatchStatus.Finished
        });
        
        var client = _authFactory.CreateClient();
        
        var response = await client.GetAsync($"watchlist/other-users-item", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }


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
    public async Task AddWatchlistItem_Returns201AndCreatedItem_WhenUnauthorized()
    {
        const int tmdbId = 1;
        var requestBody = new AddWatchlistItemRequest(TmbdId: tmdbId, WatchStatus.WantToWatch);
        var client = _authFactory.CreateClient();
     
        var response = await client.PostAsJsonAsync("/watchlist", requestBody, cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var items = await response.Content.ReadFromJsonAsync<List<WatchlistItemResponse>>(TestContext.Current.CancellationToken);
        
        Assert.NotNull(items);
        Assert.Single(items);
        Assert.Contains(items, i => i.Movie.TmdbId == tmdbId);
    }

    [Fact]
    public async Task AddWatchlistItem_Returns400WhenTmdbIdIsInvalid_WhenUnauthorized()
    {
        var requestBody = new AddWatchlistItemRequest(TmbdId: 0, WatchStatus.WantToWatch);
        var client = _authFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/watchlist", requestBody, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    
    [Fact]
    public async Task AddWatchlistItem_Returns400WhenWatchStatusIsNotAValidValue_WhenUnauthorized()
    {
        var json = """{ "tmdbId": 1, "watchStatus": "NotAStatus" }""";
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var client = _authFactory.CreateClient();

        var response = await client.PostAsync(
            "/watchlist", content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Fact]
    public async Task AddWatchlistItem_Returns409WhenItemIsAlreadyInTheUsersList_WhenUnauthorized()
    {
        await SeedAsync(new MovieWatchlistItem
        {
            UserId = TestAuthHandler.TestUserId,
            MovieId = 1,
            WatchStatus = WatchStatus.WantToWatch
        });
        const int tmdbId = 1;
        var requestBody = new AddWatchlistItemRequest(TmbdId: tmdbId, WatchStatus.Finished);
        var client = _authFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/watchlist", requestBody,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [Fact]
    public async Task AddWatchlistItem_Returns401_WhenUnauthorized()
    {
        var requestBody = new AddWatchlistItemRequest(1, WatchStatus.WantToWatch);
        var client = _factory.CreateClient();
        
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