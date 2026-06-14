using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Tests.Helpers;

namespace Tests.Api;


public class AuthEndpointsTests(RealAuthWebAppFactory authFactory) : IClassFixture<RealAuthWebAppFactory>
{
    private readonly RealAuthWebAppFactory _authFactory = authFactory;
    private static string UniqueEmail() => $"user-{Guid.NewGuid():N}@example.com";
    private const string ValidPassword = "Passw0rd!";

    /*
     * Tests for register POST request "/auth/register"
     * Register a new user
     */
    
    [Fact]
    public async Task Register_Returns200_WhenCredentialsAreValid()
    {
        var client = _authFactory.CreateClient();
        var body = new { email = UniqueEmail(), password = ValidPassword };

        var response = await client.PostAsJsonAsync(
            "/auth/register", body, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_Returns400_WhenPasswordIsTooWeak()
    {
        var client = _authFactory.CreateClient();
        var body = new { email = UniqueEmail(), password = "123" }; // violates default rules

        var response = await client.PostAsJsonAsync(
            "/auth/register", body, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    /*
     * Tests for login POST request "/auth/login"
     * Log in, receive a bearer token
     */

    [Fact]
    public async Task Login_Returns200AndToken_WhenCredentialsAreValid()
    {
        var client = _authFactory.CreateClient();
        var creds = new { email = UniqueEmail(), password = ValidPassword };
        var ct = TestContext.Current.CancellationToken;

        await client.PostAsJsonAsync("/auth/register", creds, ct);

        var response = await client.PostAsJsonAsync("/auth/login", creds, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = await response.Content.ReadFromJsonAsync<TokenResponse>(ct);
        Assert.NotNull(token);
        Assert.False(string.IsNullOrWhiteSpace(token.AccessToken));
    }

    [Fact]
    public async Task Login_Returns401_WhenCredentialsAreInvalid()
    {
        var client = _authFactory.CreateClient();
        var creds = new { email = UniqueEmail(), password = "WrongPass1!" }; // never registered

        var response = await client.PostAsJsonAsync(
            "/auth/login", creds, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_GrantsAccessToProtectedEndpoint()
    {
        var client = _authFactory.CreateClient();
        var creds = new { email = UniqueEmail(), password = ValidPassword };
        var ct = TestContext.Current.CancellationToken;

        await client.PostAsJsonAsync("/auth/register", creds, ct);
        var login = await client.PostAsJsonAsync("/auth/login", creds, ct);
        var token = await login.Content.ReadFromJsonAsync<TokenResponse>(ct);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        var response = await client.GetAsync("/watchlist", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_Returns401_WhenUnauthenticated()
    {
        var client = _authFactory.CreateClient();

        var response = await client.PostAsync(
            "/auth/logout", null, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    /*
     * Tests for logout POST request "/auth/logout"
     * Logs out the user, revokes the token
     */


    [Fact]
    public async Task Logout_Returns200_WhenAuthenticated()
    {
        var client = _authFactory.CreateClient();
        var creds = new { email = UniqueEmail(), password = ValidPassword };
        var ct = TestContext.Current.CancellationToken;

        await client.PostAsJsonAsync("/auth/register", creds, ct);
        var login = await client.PostAsJsonAsync("/auth/login", creds, ct);
        var token = await login.Content.ReadFromJsonAsync<TokenResponse>(ct);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        var response = await client.PostAsync("/auth/logout", null, ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Logout_RevokesToken_SoItNoLongerGrantsAccess()
    {
        var client = _authFactory.CreateClient();
        var creds = new { email = UniqueEmail(), password = ValidPassword };
        var ct = TestContext.Current.CancellationToken;

        await client.PostAsJsonAsync("/auth/register", creds, ct);
        var login = await client.PostAsJsonAsync("/auth/login", creds, ct);
        var token = await login.Content.ReadFromJsonAsync<TokenResponse>(ct);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        var before = await client.GetAsync("/watchlist", ct);
        Assert.Equal(HttpStatusCode.OK, before.StatusCode);

        var logout = await client.PostAsync("/auth/logout", null, ct);
        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);

        var after = await client.GetAsync("/watchlist", ct);
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
    }
}
