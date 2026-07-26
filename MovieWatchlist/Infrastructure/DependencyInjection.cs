using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Domain.Entities;
using MovieWatchlist.Infrastructure.Identity;
using MovieWatchlist.Infrastructure.Persistence;
using MovieWatchlist.Infrastructure.Repositories;

namespace MovieWatchlist.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IMovieRepository<Movie>, InMemoryMovieRepository>();
        services.AddSingleton<IWatchlistRepository<MovieWatchlistItem>, InMemoryMovieWatchlistRepository>();

        // TODO: in-memory store as a placeholder; database-setup issue swaps in SQL Server + migrations.
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("MovieWatchlist"));

        services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IRevokedTokenStore, InMemoryRevokedTokenStore>();

        return services;
    }
}