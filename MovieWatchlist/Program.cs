using MovieWatchlist.Api.Endpoints;
using MovieWatchlist.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();


app.MapWatchlistEndpoints();
app.MapMovieEndpoints();
app.MapAuthEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

public partial class Program { };