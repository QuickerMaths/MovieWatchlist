using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Application.Contracts;
using MovieWatchlist.Infrastructure.Identity;

namespace MovieWatchlist.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("auth");

        /*
         * Endpoint for login POST request "/auth/login"
         * Log in, receive a bearer token
         */
        group.MapPost("login", async (
            [FromBody] LoginRequest request,
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
                return Results.Unauthorized();

            return Results.Ok(new { accessToken = tokenService.CreateToken(user) });
        });
        
        /*
         * Endpoint for logout POST request "/auth/logout"
         * Logs out the user, revokes the token
         */
        group.MapPost("logout", Results.NoContent);
        
        /*
         * Endpoint for register POST request "/auth/register"
         * Register a new user
         */
        group.MapPost("register", async (
            [FromBody] RegisterRequest request,
            UserManager<ApplicationUser> userManager) =>
        {
            var user = new ApplicationUser { UserName = request.Email, Email = request.Email };
            var result = await userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
                return Results.Ok();

            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            return Results.ValidationProblem(errors);
        });

        return app;
    }
}