using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;
using MovieWatchlist.Application.Abstractions;
using MovieWatchlist.Domain.Entities;

namespace MovieWatchlist.Application.Services;

public class TmdbClient(HttpClient httpClient) : ITmdbClient
{
    public async Task<IEnumerable<Movie>> SearchAsync(string query, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"search/movie?query={Uri.EscapeDataString(query)}", ct);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<SearchResponseDto>(ct);
        return payload?.Results.Select(ToMovie) ?? [];
    }

    public async Task<Movie?> GetByIdAsync(int tmdbId, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync($"movie/{tmdbId}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<MovieDto>(ct);
        return dto is null ? null : ToMovie(dto);
    }


    private static Movie ToMovie(MovieDto dto) => new()
    {
        Id = dto.Id.ToString(),
        TmdbId = dto.Id,
        Title = dto.Title,
        Overview = dto.Overview ?? string.Empty,
        PosterPath = dto.PosterPath ?? string.Empty,
        ReleaseDate = DateTime.TryParse(
            dto.ReleaseDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : default
    };

    private sealed record MovieDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("overview")] string? Overview,
        [property: JsonPropertyName("poster_path")] string? PosterPath,
        [property: JsonPropertyName("release_date")] string? ReleaseDate);

    private sealed record SearchResponseDto(
        [property: JsonPropertyName("results")] IReadOnlyList<MovieDto> Results);
}
