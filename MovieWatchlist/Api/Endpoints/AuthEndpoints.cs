using Microsoft.AspNetCore.Mvc;
using MovieWatchlist.Application.Contracts;

namespace MovieWatchlist.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("auth");

        group.MapPost("login", ([FromBody]LoginRequest request ) => Results.Ok());
        group.MapPost("logout", Results.NoContent);
        group.MapPost("register", ([FromBody] RegisterRequest request) => Results.Ok());

        return app;
    }
}