using System.Net;
using System.Text;
using MovieWatchlist.Application.Services;

namespace Tests.Unit;

public class TmdbClientTests
{
    private const string SearchJson = """
        {
            "results": [
                {
                    "id": 27205,
                    "title": "Inception",
                    "overview": "A thief who steals corporate secrets.",
                    "poster_path": "/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg",
                    "release_date": "2010-07-16"
                }
            ]
        }
        """;

    private const string MovieJson = """
        {
            "id": 27205,
            "title": "Inception",
            "overview": "A thief who steals corporate secrets.",
            "poster_path": "/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg",
            "release_date": "2010-07-16"
        }
        """;

    private static (TmdbClient client, FakeHttpHandler handler) Build(
        HttpStatusCode status, string json)
    {
        var handler = new FakeHttpHandler(status, json);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/")
        };
        return (new TmdbClient(httpClient), handler);
    }
    
    /*
    * Tests for SearchAsync method
    */
    
    [Fact]
    public async Task SearchAsync_MapsSnakeCaseFieldsCorrectly()
    {
        var (client, _) = Build(HttpStatusCode.OK, SearchJson);

        var results = (await client.SearchAsync("inception", CancellationToken.None)).ToList();

        Assert.Single(results);
        var movie = results[0];
        Assert.Equal(27205, movie.TmdbId);
        Assert.Equal("Inception", movie.Title);
        Assert.Equal("A thief who steals corporate secrets.", movie.Overview);
        Assert.Equal("/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg", movie.PosterPath);
        Assert.Equal(new DateTime(2010, 7, 16), movie.ReleaseDate);
    }

    [Fact]
    public async Task SearchAsync_SendsQueryInRequest()
    {
        var (client, handler) = Build(HttpStatusCode.OK, SearchJson);

        await client.SearchAsync("inception", CancellationToken.None);

        Assert.NotNull(handler.LastRequest);
        Assert.Contains("query=inception", handler.LastRequest!.RequestUri!.Query);
    }
    
    /*
     * Tests for GetByIdAsync method
     */

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var (client, _) = Build(HttpStatusCode.NotFound, "{}");

        var result = await client.GetByIdAsync(99999, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_MapsSnakeCaseFieldsCorrectly()
    {
        var (client, _) = Build(HttpStatusCode.OK, MovieJson);

        var movie = await client.GetByIdAsync(27205, CancellationToken.None);

        Assert.NotNull(movie);
        Assert.Equal(27205, movie!.TmdbId);
        Assert.Equal("Inception", movie.Title);
        Assert.Equal("A thief who steals corporate secrets.", movie.Overview);
        Assert.Equal("/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg", movie.PosterPath);
        Assert.Equal(new DateTime(2010, 7, 16), movie.ReleaseDate);
    }

    private sealed class FakeHttpHandler(HttpStatusCode status, string json) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }
}
