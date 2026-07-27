using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MovieWatchlist.Api.Endpoints;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Infrastructure;
using MovieWatchlist.Infrastructure.Persistence;
using MovieWatchlist.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWTSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((options, jwtSettings) =>
    {
        var settings = jwtSettings.Value;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,            ValidIssuer = settings.Issuer,
            ValidateAudience = true,          ValidAudience = settings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key))
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var revokedTokens = context.HttpContext.RequestServices
                    .GetRequiredService<IRevokedTokenStore>();
                var jti = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Jti);

                if (jti is not null && await revokedTokens.IsRevokedAsync(jti, context.HttpContext.RequestAborted))
                    context.Fail("Token has been revoked.");
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
}

app.MapWatchlistEndpoints();
app.MapMovieEndpoints();
app.MapAuthEndpoints();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

public partial class Program { };