using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Application.Services;
using MovieWatchlist.Domain.Entities;
using MovieWatchlist.Infrastructure.Identity;
using MovieWatchlist.Infrastructure.Persistence;
using MovieWatchlist.Infrastructure.Repositories;

namespace MovieWatchlist.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IMovieRepository<Movie>, EfMovieRepository>();
        services.AddScoped<IWatchlistRepository<MovieWatchlistItem>, EfWatchlistRepository>();

        // TODO: in-memory store as a placeholder; database-setup issue swaps in SQL Server + migrations.
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("MovieWatchlist"));

        services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IRevokedTokenStore, InMemoryRevokedTokenStore>();

        services.AddScoped<WatchlistService>();
        services.AddHttpClient<ITmdbClient, TmdbClient>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            client.BaseAddress = new Uri(config["Tmdb:BaseUrl"] ?? "https://api.themoviedb.org/3/");

            var apiKey = config["Tmdb:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        });

        return services;
    }
}