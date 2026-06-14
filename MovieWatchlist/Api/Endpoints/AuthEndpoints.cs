using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Application.Contracts;

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
        group.MapPost("login", ([FromBody]LoginRequest request ) => Results.Ok());
        
        /*
         * Endpoint for logout POST request "/auth/logout"
         * Logs out the user, revokes the token
         */
        group.MapPost("logout", Results.NoContent);
        
        /*
         * Endpoint for register POST request "/auth/register"
         * Register a new user
         */
        group.MapPost("register", ([FromBody] RegisterRequest request) => Results.Ok());

        return app;
    }
}