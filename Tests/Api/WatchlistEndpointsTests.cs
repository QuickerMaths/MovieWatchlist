using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Application.Contracts.MovieWatchlist;
using MovieWatchlist.Domain.Entities;
using MovieWatchlist.Infrastructure.Persistence;
using NSubstitute;
using Tests.Helpers;

namespace Tests.Api;

public class WatchlistEndpointTests(
    FakeAuthWebAppFactory authFactory,
    RealAuthWebAppFactory plainFactory)
    :
        IClassFixture<FakeAuthWebAppFactory>,
        IClassFixture<RealAuthWebAppFactory>,
        IDisposable
{
    private readonly FakeAuthWebAppFactory _authFactory = authFactory;
    private readonly RealAuthWebAppFactory _factory = plainFactory;
    
    public void Dispose()
    {
        using var scope = _authFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
    }

    private async Task SeedAsync(params MovieWatchlistItem[] items)
    {
        using var scope = _authFactory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IWatchlistRepository<MovieWatchlistItem>>();

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
    public async Task GetWatchlistItemById_Returns200AndSingleItem_WhenAuthenticated()
    {
        const string itemId = "item-1";
        await SeedAsync(new MovieWatchlistItem
        {
            Id = itemId,
            UserId = TestAuthHandler.TestUserId,
            MovieId = 1,
            WatchStatus = WatchStatus.WantToWatch
        });

        var client = _authFactory.CreateClient();

        var response = await client.GetAsync($"/watchlist/{itemId}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var item = await response.Content.ReadFromJsonAsync<WatchlistItemResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(item);
        Assert.Equal(WatchStatus.WantToWatch, item.WatchStatus);
    }

    [Fact]
    public async Task GetWatchlistItemById_Returns404WhenItemIsNotFound_WhenAuthenticated()
    {
        var client = _authFactory.CreateClient();
        
        var response = await client.GetAsync($"watchlist/1", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetWatchlistItemById_Returns404WhenTheItemIsNotOwnedByTheUser_WhenAuthenticated()
    {
        const string someOtherUserId = "some-other-user-id";
        await SeedAsync(new MovieWatchlistItem
        {
            Id = someOtherUserId,
            UserId = "otherUsersId",
            MovieId = 1,
            WatchStatus = WatchStatus.Finished
        });
        var client = _authFactory.CreateClient();
        
        var response = await client.GetAsync($"watchlist/{someOtherUserId}", TestContext.Current.CancellationToken);
        
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
    public async Task AddWatchlistItem_Returns201AndCreatedItem_WhenAuthenticated()
    {
        const int tmdbId = 1;
        _authFactory.TmdbClient.GetByIdAsync(tmdbId, Arg.Any<CancellationToken>())
            .Returns(new Movie
            {
                TmdbId = tmdbId,
                Title = "inception",
                Overview = "inception movie",
                PosterPath = "poster-path",
                ReleaseDate = new DateTime(2010, 7, 16)
            });
        var requestBody = new AddWatchlistItemRequest(TmbdId: tmdbId, WatchStatus.WantToWatch);
        var client = _authFactory.CreateClient();

        var response = await client.PostAsJsonAsync("/watchlist", requestBody, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var item = await response.Content.ReadFromJsonAsync<WatchlistItemResponse>(TestContext.Current.CancellationToken);

        Assert.NotNull(item);
        Assert.Equal(tmdbId, item.Movie.TmdbId);
    }

    [Fact]
    public async Task AddWatchlistItem_Returns400WhenTmdbIdIsInvalid_WhenAuthenticated()
    {
        var requestBody = new AddWatchlistItemRequest(TmbdId: 0, WatchStatus.WantToWatch);
        var client = _authFactory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/watchlist", requestBody, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    
    [Fact]
    public async Task AddWatchlistItem_Returns400WhenWatchStatusIsNotAValidValue_WhenAuthenticated()
    {
        var json = """{ "tmbdId": 1, "status": "NotAStatus" }""";
        var requestBody = new StringContent(json, Encoding.UTF8, "application/json");
        var client = _authFactory.CreateClient();

        var response = await client.PostAsync(
            "/watchlist", requestBody, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }


    [Fact]
    public async Task AddWatchlistItem_Returns409WhenItemIsAlreadyInTheUsersList_WhenAuthenticated()
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
    public async Task UpdateWatchlistItem_Returns204_WhenAuthenticated()
    {
        var requestBody = new UpdateWatchlistItemRequest(WatchStatus: WatchStatus.Watching, Rating: null, Note: null);
        const string watchlistItemId = "items-id";
        await SeedAsync(new MovieWatchlistItem
        {
            Id = watchlistItemId,
            UserId = TestAuthHandler.TestUserId,
            MovieId = 1,
            WatchStatus = WatchStatus.WantToWatch
        });
        var client = _authFactory.CreateClient();
        
        var response = await client.PutAsJsonAsync($"/watchlist/{watchlistItemId}", requestBody, TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateWatchlistItem_Returns400WhenRatingIsOutOfRange_WhenAuthenticated()
    {
        const string itemId = "my-item-id"; 
        await SeedAsync(new MovieWatchlistItem
        {
            Id = itemId,
            UserId = TestAuthHandler.TestUserId,
            MovieId = 1,
            WatchStatus = WatchStatus.WantToWatch
        });
        var requestBody = new UpdateWatchlistItemRequest(
            WatchStatus: WatchStatus.Finished,
            Rating: 11,
            Note: "Great movie");
        var client = _authFactory.CreateClient();

        var response = await client.PutAsJsonAsync(
            $"/watchlist/{itemId}", requestBody, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [Fact]
    public async Task UpdateWatchlistItem_Returns400WhenWatchStatusIsNotAValidValue_WhenAuthenticated()
    {
        const string itemId = "my-item-id";
        var json = """{ "watchStatus": "NotAStatus" }""";
        var requestBody = new StringContent(json, Encoding.UTF8, "application/json");
        await SeedAsync(new MovieWatchlistItem
        {
            Id = itemId,
            UserId = TestAuthHandler.TestUserId,
            MovieId = 1,
            WatchStatus = WatchStatus.WantToWatch
        });
        var client = _authFactory.CreateClient();

        var response = await client.PutAsync(
            $"/watchlist/{itemId}", requestBody, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateWatchlistItem_Returns404WhenTheItemIsNotOwnedByTheUser_WhenAuthenticated()
    {
        var requestBody = new UpdateWatchlistItemRequest(WatchStatus: WatchStatus.Watching, Rating: null, Note: null);
        const string someOtherUserId = "some-other-user-id";
        await SeedAsync(new MovieWatchlistItem
        {
            Id = someOtherUserId,
            UserId = "otherUsersId",
            MovieId = 1,
            WatchStatus = WatchStatus.Finished
        });
        var client = _authFactory.CreateClient();
        
        var response = await client.PutAsJsonAsync($"watchlist/{someOtherUserId}", requestBody, TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    

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
    public async Task DeleteWatchlistItem_Returns204_WhenAuthenticated()
    {
        const string watchlistItemId = "items-id";
        await SeedAsync(new MovieWatchlistItem
        {
            Id = watchlistItemId,
            UserId = TestAuthHandler.TestUserId,
            MovieId = 1,
            WatchStatus = WatchStatus.WantToWatch
        });
        var client = _authFactory.CreateClient();
        
        var response = await client.DeleteAsync($"/watchlist/{watchlistItemId}", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteWatchlistItem_Returns204WhenTheItemIsNotOwnedByTheUser_WhenAuthenticated()
    {
        const string watchlistItemId = "items-id";
        await SeedAsync(new MovieWatchlistItem
        {
            Id = watchlistItemId,
            UserId = "other-user-id",
            MovieId = 1,
            WatchStatus = WatchStatus.WantToWatch
        });
        var client = _authFactory.CreateClient();
        
        var response = await client.DeleteAsync($"/watchlist/{watchlistItemId}", TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }


    [Fact]
    public async Task DeleteWatchlistItem_Returns401_WhenUnauthorized()
    {
        var client = _factory.CreateClient();
        
        var response = await client.DeleteAsync($"watchlist/1", cancellationToken: TestContext.Current.CancellationToken);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}