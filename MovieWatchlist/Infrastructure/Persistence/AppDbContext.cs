using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieWatchlist.Domain.Entities;
using MovieWatchlist.Infrastructure.Identity;

namespace MovieWatchlist.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<MovieWatchlistItem> WatchlistItems => Set<MovieWatchlistItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Movie>(movie =>
        {
            movie.HasKey(m => m.Id);
            movie.HasIndex(m => m.TmdbId).IsUnique();
        });

        builder.Entity<MovieWatchlistItem>(item =>
        {
            item.HasKey(i => i.Id);
            item.HasIndex(i => new { i.UserId, i.MovieId }).IsUnique();
        });
    }
}
