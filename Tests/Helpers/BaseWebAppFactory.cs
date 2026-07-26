using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MovieWatchlist.Application.Abstractions;
using NSubstitute;

namespace Tests.Helpers;

public abstract class BaseWebAppFactory: WebApplicationFactory<Program>
{
    // Shared substitute so tests can arrange TMDB responses instead of hitting the network.
    public ITmdbClient TmdbClient { get; } = Substitute.For<ITmdbClient>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JWTSettings:Key"] = "test-signing-key-at-least-32-bytes-long-0123456789",
                ["JWTSettings:Issuer"]        = "MovieWatchlist.Tests",
                ["JWTSettings:Audience"]      = "MovieWatchlist.Tests",
                ["JWTSettings:ExpiryMinutes"] = "60",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITmdbClient>();
            services.AddSingleton(TmdbClient);
        });
    }
}
