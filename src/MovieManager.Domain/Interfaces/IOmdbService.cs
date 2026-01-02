using MovieManager.Domain.Entities;

namespace MovieManager.Domain.Interfaces;

public interface IOmdbService
{
    Task<IEnumerable<OmdbSearchResult>> SearchByTitleAsync(string title);
    Task<Movie?> GetByImdbIdAsync(string imdbId);
}

public class OmdbSearchResult
{
    public string ImdbId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Poster { get; set; } = string.Empty;
}
