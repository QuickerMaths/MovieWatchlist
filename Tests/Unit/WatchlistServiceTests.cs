using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Application.Contracts.MovieWatchlist;
using MovieWatchlist.Application.Services;
using MovieWatchlist.Domain.Entities;
using MovieWatchlist.Infrastructure.Repositories;
using NSubstitute;

namespace Tests.Unit;

public class WatchlistServiceTests
{
    private static Movie SampleMovie(int tmdbId = 1) => new()
    {
        TmdbId = tmdbId,
        Title = "Inception",
        Overview = "A thief who steals corporate secrets.",
        PosterPath = "/poster.jpg",
        ReleaseDate = new DateTime(2010, 7, 16)
    };

    private static (WatchlistService sut,
                    InMemoryMovieWatchlistRepository watchlistRepo,
                    InMemoryMovieRepository movieRepo,
                    ITmdbClient tmdbClient) Build()
    {
        var watchlistRepo = new InMemoryMovieWatchlistRepository();
        var movieRepo = new InMemoryMovieRepository();
        var tmdbClient = Substitute.For<ITmdbClient>();
        var sut = new WatchlistService(watchlistRepo, movieRepo, tmdbClient);
        return (sut, watchlistRepo, movieRepo, tmdbClient);
    }
    
    /*
     * Tests for AddItemAsync method
     */
    
    [Fact]
    public async Task AddItem_CreatesItemForUser_AndReturnsResponse()
    {
        var (sut, watchlistRepo, movieRepo, _) = Build();
        var movie = SampleMovie();
        await movieRepo.AddAsync(movie);

        var response = await sut.AddItemAsync(
            "user-1", new AddWatchlistItemRequest(movie.TmdbId, WatchStatus.WantToWatch), CancellationToken.None);

        Assert.NotNull(response);
        Assert.False(string.IsNullOrEmpty(response.Id));
        Assert.Equal(WatchStatus.WantToWatch, response.WatchStatus);
        Assert.Equal(movie.TmdbId, response.Movie.TmdbId);

        var items = await watchlistRepo.GetByUserAsync("user-1");
        Assert.Single(items);
    }

    [Fact]
    public async Task AddItem_FetchesAndCachesMovie_WhenMovieNotCached()
    {
        var (sut, _, movieRepo, tmdbClient) = Build();
        var movie = SampleMovie();
        tmdbClient.GetByIdAsync(movie.TmdbId, Arg.Any<CancellationToken>()).Returns(movie);

        await sut.AddItemAsync("user-1", new AddWatchlistItemRequest(movie.TmdbId, WatchStatus.WantToWatch), CancellationToken.None);

        await tmdbClient.Received(1).GetByIdAsync(movie.TmdbId, Arg.Any<CancellationToken>());
        Assert.True(await movieRepo.ExistsAsync(movie.TmdbId));
        var result = movieRepo.GetByTmdbIdAsync(movie.TmdbId).Result;
        Assert.True(result != null && result.TmdbId == movie.TmdbId);
    }

    [Fact]
    public async Task AddItem_DoesNotCallTmdb_WhenMovieAlreadyCached()
    {
        var (sut, _, movieRepo, tmdbClient) = Build();
        var movie = SampleMovie();
        await movieRepo.AddAsync(movie);

        await sut.AddItemAsync("user-1", new AddWatchlistItemRequest(movie.TmdbId, WatchStatus.WantToWatch), CancellationToken.None);

        await tmdbClient.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddItem_Throws_WhenUserAlreadyHasTheMovie()
    {
        var (sut, _, movieRepo, _) = Build();
        var movie = SampleMovie();
        await movieRepo.AddAsync(movie);
        var request = new AddWatchlistItemRequest(movie.TmdbId, WatchStatus.WantToWatch);
        await sut.AddItemAsync("user-1", request, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.AddItemAsync("user-1", request, CancellationToken.None));
    }
    
    /*
     * Tests for GetItemsAsync method
     */

    [Fact]
    public async Task GetItems_ReturnsOnlyUsersItems()
    {
        var (sut, watchlistRepo, movieRepo, _) = Build();
        await movieRepo.AddAsync(SampleMovie(1));
        await movieRepo.AddAsync(SampleMovie(2));
        await watchlistRepo.AddAsync(new MovieWatchlistItem { UserId = "user-1", MovieId = 1, WatchStatus = WatchStatus.WantToWatch });
        await watchlistRepo.AddAsync(new MovieWatchlistItem { UserId = "user-2", MovieId = 2, WatchStatus = WatchStatus.Finished });

        var items = (await sut.GetItemsAsync("user-1", null, CancellationToken.None)).ToList();

        Assert.Single(items);
        Assert.Equal(1, items[0].Movie.TmdbId);
    }

    [Fact]
    public async Task GetItems_ReturnsOnlyMatchingStatus_WhenFilterProvided()
    {
        var (sut, watchlistRepo, movieRepo, _) = Build();
        await movieRepo.AddAsync(SampleMovie(1));
        await movieRepo.AddAsync(SampleMovie(2));
        await watchlistRepo.AddAsync(new MovieWatchlistItem { UserId = "user-1", MovieId = 1, WatchStatus = WatchStatus.WantToWatch });
        await watchlistRepo.AddAsync(new MovieWatchlistItem { UserId = "user-1", MovieId = 2, WatchStatus = WatchStatus.Finished });

        var items = (await sut.GetItemsAsync("user-1", WatchStatus.Finished, CancellationToken.None)).ToList();

        Assert.Single(items);
        Assert.All(items, i => Assert.Equal(WatchStatus.Finished, i.WatchStatus));
    }

    [Fact]
    public async Task GetItems_MapsMovieFieldsCorrectly()
    {
        var (sut, watchlistRepo, movieRepo, _) = Build();
        var movie = SampleMovie();
        await movieRepo.AddAsync(movie);
        await watchlistRepo.AddAsync(new MovieWatchlistItem { UserId = "user-1", MovieId = movie.TmdbId, WatchStatus = WatchStatus.WantToWatch });

        var items = (await sut.GetItemsAsync("user-1", null, CancellationToken.None)).ToList();

        Assert.Single(items);
        var response = items[0];
        Assert.NotEmpty(response.Id);
        Assert.Equal(movie.TmdbId, response.Movie.TmdbId);
        Assert.Equal(movie.Title, response.Movie.Title);
        Assert.Equal(movie.Overview, response.Movie.Overview);
        Assert.Equal(movie.PosterPath, response.Movie.PosterPath);
    }
    
    /*
     * Tests for GetItemByIdAsync method
     */
    
    [Fact]
    public async Task GetItemById_ReturnsItem_WhenOwnedByUser()
    {
        var (sut, watchlistRepo, movieRepo, _) = Build();
        var movie = SampleMovie();
        await movieRepo.AddAsync(movie);
        await watchlistRepo.AddAsync(new MovieWatchlistItem
            { Id = "item-1", UserId = "user-1", MovieId = movie.TmdbId, WatchStatus = WatchStatus.WantToWatch });

        var item = await sut.GetItemByIdAsync("item-1", "user-1", CancellationToken.None);

        Assert.NotNull(item);
        Assert.Equal("item-1", item.Id);
        Assert.Equal(movie.TmdbId, item.Movie.TmdbId);
    }

    [Fact]
    public async Task GetItemById_ReturnsNull_WhenItemDoesNotExist()
    {
        var (sut, _, _, _) = Build();

        var item = await sut.GetItemByIdAsync("missing", "user-1", CancellationToken.None);

        Assert.Null(item);
    }

    [Fact]
    public async Task GetItemById_ReturnsNull_WhenItemBelongsToAnotherUser()
    {
        var (sut, watchlistRepo, movieRepo, _) = Build();
        var movie = SampleMovie();
        await movieRepo.AddAsync(movie);
        await watchlistRepo.AddAsync(new MovieWatchlistItem
            { Id = "item-1", UserId = "other-user", MovieId = movie.TmdbId, WatchStatus = WatchStatus.WantToWatch });

        var item = await sut.GetItemByIdAsync("item-1", "user-1", CancellationToken.None);

        Assert.Null(item);
    }
    
    /*
     * Tests for UpdateItemAsync method
     */

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-1)]
    public async Task UpdateItem_Throws_WhenRatingIsOutOfRange(int invalidRating)
    {
        var (sut, watchlistRepo, movieRepo, _) = Build();
        var movie = SampleMovie();
        await movieRepo.AddAsync(movie);
        await watchlistRepo.AddAsync(new MovieWatchlistItem { Id = "item-1", UserId = "user-1", MovieId = movie.TmdbId, WatchStatus = WatchStatus.WantToWatch });

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => sut.UpdateItemAsync("item-1", "user-1", new UpdateWatchlistItemRequest(WatchStatus.Finished, invalidRating, null), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateItem_Succeeds_WhenRatingIsNull()
    {
        var (sut, watchlistRepo, movieRepo, _) = Build();
        var movie = SampleMovie();
        await movieRepo.AddAsync(movie);
        await watchlistRepo.AddAsync(new MovieWatchlistItem { Id = "item-1", UserId = "user-1", MovieId = movie.TmdbId, WatchStatus = WatchStatus.WantToWatch });

        var ex = await Record.ExceptionAsync(
            () => sut.UpdateItemAsync("item-1", "user-1", new UpdateWatchlistItemRequest(WatchStatus.Finished, null, null), CancellationToken.None));

        Assert.Null(ex);
    }
    
    [Fact]
    public async Task UpdateItem_Throws_WhenItemDoesNotExist()
    {
        var (sut, _, _, _) = Build();
        var request = new UpdateWatchlistItemRequest(WatchStatus.Finished, 5, null); // rating poprawny!

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.UpdateItemAsync("missing", "user-1", request, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateItem_Throws_WhenItemBelongsToAnotherUser()
    {
        var (sut, watchlistRepo, movieRepo, _) = Build();
        var movie = SampleMovie();
        await movieRepo.AddAsync(movie);
        await watchlistRepo.AddAsync(new MovieWatchlistItem
            { Id = "item-1", UserId = "other-user", MovieId = movie.TmdbId, WatchStatus = WatchStatus.WantToWatch });
        var request = new UpdateWatchlistItemRequest(WatchStatus.Finished, 5, null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.UpdateItemAsync("item-1", "user-1", request, CancellationToken.None));
    }
    
    /*
     * Tests for DeleteItemAsync method
     */
    
    [Fact]
    public async Task DeleteItem_RemovesItem_WhenOwnedByUser()
    {
        var (sut, watchlistRepo, movieRepo, _) = Build();
        var movie = SampleMovie();
        await movieRepo.AddAsync(movie);
        await watchlistRepo.AddAsync(new MovieWatchlistItem
            { Id = "item-1", UserId = "user-1", MovieId = movie.TmdbId, WatchStatus = WatchStatus.WantToWatch });

        await sut.DeleteItemAsync("item-1", "user-1", CancellationToken.None);

        var remaining = await watchlistRepo.GetByUserAsync("user-1");
        Assert.Empty(remaining);
    }

    [Fact]
    public async Task DeleteItem_DoesNotRemoveItem_WhenOwnedByAnotherUser()
    {
        var (sut, watchlistRepo, movieRepo, _) = Build();
        var movie = SampleMovie();
        await movieRepo.AddAsync(movie);
        await watchlistRepo.AddAsync(new MovieWatchlistItem
            { Id = "item-1", UserId = "other-user", MovieId = movie.TmdbId, WatchStatus = WatchStatus.WantToWatch });

        // usuwamy jako user-1 — cudzy element musi przetrwać
        await sut.DeleteItemAsync("item-1", "user-1", CancellationToken.None);

        var survived = await watchlistRepo.GetByIdAsync("item-1", "other-user");
        Assert.NotNull(survived);
    }
}
