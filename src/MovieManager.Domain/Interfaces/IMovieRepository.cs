using MovieManager.Domain.Entities;

namespace MovieManager.Domain.Interfaces;

public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(string id);
    Task<Movie?> GetByExternalIdAsync(string externalId);
    Task<IEnumerable<Movie>> GetAllAsync(int page, int pageSize);
    Task<IEnumerable<Movie>> SearchByTitleAsync(string title, int page, int pageSize);
    Task<IEnumerable<Movie>> GetByGenreAsync(string genre, int page, int pageSize);
    Task<IEnumerable<Movie>> GetByDirectorAsync(string director, int page, int pageSize);
    Task<Movie?> GetRandomAsync();
    Task<IEnumerable<Movie>> GetRecommendationsByGenreAsync(string genre, int count);
    Task<long> GetTotalCountAsync();
    Task<Dictionary<string, int>> GetCountByGenreAsync();
    Task<Dictionary<int, int>> GetCountByYearAsync();
    Task<Dictionary<string, int>> GetTopDirectorsAsync(int count);
    Task<Movie> CreateAsync(Movie movie);
    Task<Movie> UpdateAsync(Movie movie);
    Task<bool> SoftDeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}
