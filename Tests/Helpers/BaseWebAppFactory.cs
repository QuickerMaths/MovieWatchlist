using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Tests.Helpers;

public abstract class BaseWebAppFactory: WebApplicationFactory<Program>
{
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
    }
}