using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Infrastructure.Persistence;
using NSubstitute;

namespace Tests.Helpers;

public abstract class BaseWebAppFactory: WebApplicationFactory<Program>
{
    // Shared substitute so tests can arrange TMDB responses instead of hitting the network.
    public ITmdbClient TmdbClient { get; } = Substitute.For<ITmdbClient>();

    private readonly string _databaseName = $"tests-{Guid.NewGuid()}";

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
            
            var toRemove = services.Where(d =>
                d.ServiceType == typeof(AppDbContext) ||
                d.ServiceType == typeof(DbContextOptions) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GetGenericArguments().Contains(typeof(AppDbContext)))).ToList();

            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));
        });
    }
}
