using MovieWatchlist.Infrastructure.Identity;

namespace MovieWatchlist.Application.Abstractions;

public interface ITokenService
{
    string CreateToken(ApplicationUser user);
}
