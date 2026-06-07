using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Domain.Entities;
using MovieWatchlist.Infrastructure.Repositories;

namespace MovieWatchlist.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IMovieRepository<Movie>, InMemoryMovieRepository>();
        services.AddSingleton<IWatchlistRepository<MovieWatchlistItem>, InMemoryMovieWatchlistRepository>();

        return services;
    }
}