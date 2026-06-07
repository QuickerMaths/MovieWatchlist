using MovieWatchlist.Abstractions;
using MovieWatchlist.Entities;
using MovieWatchlist.Repositories;

namespace MovieWatchlist.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IMovieRepository<Movie>, IMovieRepository<Movie>>();
        services.AddSingleton<IWatchlistRepository<MovieWatchlistItem>, InMemoryMovieWatchlistRepository>();

        return services;
    }
}