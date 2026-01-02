using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Interfaces;
using MovieManager.Infrastructure.Settings;

namespace MovieManager.Infrastructure.External;

public class OmdbService : IOmdbService
{
    private readonly HttpClient _httpClient;
    private readonly OmdbSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public OmdbService(HttpClient httpClient, IOptions<OmdbSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<IEnumerable<OmdbSearchResult>> SearchByTitleAsync(string title)
    {
        var url = $"{_settings.BaseUrl}?apikey={_settings.ApiKey}&s={Uri.EscapeDataString(title)}&type=movie";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var searchResponse = JsonSerializer.Deserialize<OmdbSearchResponse>(content, _jsonOptions);

        if (searchResponse?.Response != "True" || searchResponse.Search == null)
            return Enumerable.Empty<OmdbSearchResult>();

        return searchResponse.Search.Select(s => new OmdbSearchResult
        {
            ImdbId = s.ImdbID,
            Title = s.Title,
            Year = s.Year,
            Type = s.Type,
            Poster = s.Poster
        });
    }

    public async Task<Movie?> GetByImdbIdAsync(string imdbId)
    {
        var url = $"{_settings.BaseUrl}?apikey={_settings.ApiKey}&i={imdbId}&plot=full";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var movieResponse = JsonSerializer.Deserialize<OmdbMovieResponse>(content, _jsonOptions);

        if (movieResponse?.Response != "True")
            return null;

        return new Movie
        {
            ExternalId = movieResponse.ImdbID,
            Title = movieResponse.Title,
            Description = movieResponse.Plot ?? string.Empty,
            Year = int.TryParse(movieResponse.Year, out var year) ? year : 0,
            Genres = ParseList(movieResponse.Genre),
            Director = movieResponse.Director ?? string.Empty,
            Actors = ParseList(movieResponse.Actors),
            Rating = ParseRating(movieResponse.ImdbRating),
            Duration = ParseDuration(movieResponse.Runtime),
            Poster = movieResponse.Poster ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            Metadata = new Dictionary<string, object>
            {
                { "imdbVotes", movieResponse.ImdbVotes ?? string.Empty },
                { "rated", movieResponse.Rated ?? string.Empty },
                { "released", movieResponse.Released ?? string.Empty },
                { "writer", movieResponse.Writer ?? string.Empty },
                { "language", movieResponse.Language ?? string.Empty },
                { "country", movieResponse.Country ?? string.Empty },
                { "awards", movieResponse.Awards ?? string.Empty }
            }
        };
    }

    private static List<string> ParseList(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value == "N/A")
            return new List<string>();

        return value.Split(',')
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }

    private static decimal ParseRating(string? rating)
    {
        if (string.IsNullOrWhiteSpace(rating) || rating == "N/A")
            return 0;

        return decimal.TryParse(rating, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
    }

    private static int ParseDuration(string? runtime)
    {
        if (string.IsNullOrWhiteSpace(runtime) || runtime == "N/A")
            return 0;

        var numbers = new string(runtime.Where(char.IsDigit).ToArray());
        return int.TryParse(numbers, out var result) ? result : 0;
    }

    private class OmdbSearchResponse
    {
        public string Response { get; set; } = string.Empty;
        public List<OmdbSearchItem>? Search { get; set; }
    }

    private class OmdbSearchItem
    {
        public string Title { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        [JsonPropertyName("imdbID")]
        public string ImdbID { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;
    }

    private class OmdbMovieResponse
    {
        public string Response { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Rated { get; set; } = string.Empty;
        public string Released { get; set; } = string.Empty;
        public string Runtime { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Director { get; set; } = string.Empty;
        public string Writer { get; set; } = string.Empty;
        public string Actors { get; set; } = string.Empty;
        public string Plot { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Awards { get; set; } = string.Empty;
        public string Poster { get; set; } = string.Empty;
        [JsonPropertyName("imdbRating")]
        public string ImdbRating { get; set; } = string.Empty;
        [JsonPropertyName("imdbVotes")]
        public string ImdbVotes { get; set; } = string.Empty;
        [JsonPropertyName("imdbID")]
        public string ImdbID { get; set; } = string.Empty;
    }
}
