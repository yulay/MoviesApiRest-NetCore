using Microsoft.Extensions.Logging;
using MovieManager.Application.Interfaces;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Enums;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Infrastructure.Services;

public interface IDataSeederService
{
    Task SeedAsync();
}

public class DataSeederService : IDataSeederService
{
    private readonly IUserRepository _userRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IOmdbService _omdbService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ILogger<DataSeederService> _logger;

    private static readonly string[] PopularMovieIds = new[]
    {
        "tt0111161", // The Shawshank Redemption
        "tt0068646", // The Godfather
        "tt0468569", // The Dark Knight
        "tt0071562", // The Godfather Part II
        "tt0050083", // 12 Angry Men
        "tt0108052", // Schindler's List
        "tt0167260", // The Lord of the Rings: The Return of the King
        "tt0110912", // Pulp Fiction
        "tt0060196", // The Good, the Bad and the Ugly
        "tt0137523", // Fight Club
        "tt0120737", // The Lord of the Rings: The Fellowship of the Ring
        "tt0109830", // Forrest Gump
        "tt1375666", // Inception
        "tt0080684", // Star Wars: Episode V - The Empire Strikes Back
        "tt0167261"  // The Lord of the Rings: The Two Towers
    };

    public DataSeederService(
        IUserRepository userRepository,
        IMovieRepository movieRepository,
        IOmdbService omdbService,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ILogger<DataSeederService> logger)
    {
        _userRepository = userRepository;
        _movieRepository = movieRepository;
        _omdbService = omdbService;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedAdminUserAsync();
        await SeedMoviesAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@moviemanager.com";

        var existingAdmin = await _userRepository.GetByEmailAsync(adminEmail);

        if (existingAdmin != null)
        {
            _logger.LogInformation("Admin user already exists, skipping creation");
            return;
        }

        var adminUser = new User
        {
            Email = adminEmail,
            PasswordHash = _passwordHasher.HashPassword("Admin123!"),
            FirstName = "Admin",
            LastName = "System",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            RefreshToken = _jwtService.GenerateRefreshToken(),
            RefreshTokenExpiry = _jwtService.GetRefreshTokenExpiry()
        };

        await _userRepository.CreateAsync(adminUser);
        _logger.LogInformation("Admin user created successfully: {Email}", adminEmail);
    }

    private async Task SeedMoviesAsync()
    {
        var totalMovies = await _movieRepository.GetTotalCountAsync();

        if (totalMovies > 0)
        {
            _logger.LogInformation("Database already contains {Count} movies, skipping seed", totalMovies);
            return;
        }

        _logger.LogInformation("Starting movie seed from OMDb API...");

        var importedCount = 0;
        var failedCount = 0;

        foreach (var imdbId in PopularMovieIds)
        {
            try
            {
                var existingMovie = await _movieRepository.GetByExternalIdAsync(imdbId);

                if (existingMovie != null)
                {
                    _logger.LogDebug("Movie {ImdbId} already exists, skipping", imdbId);
                    continue;
                }

                var movie = await _omdbService.GetByImdbIdAsync(imdbId);

                if (movie == null)
                {
                    _logger.LogWarning("Could not fetch movie {ImdbId} from OMDb", imdbId);
                    failedCount++;
                    continue;
                }

                await _movieRepository.CreateAsync(movie);
                importedCount++;
                _logger.LogDebug("Imported movie: {Title} ({Year})", movie.Title, movie.Year);

                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing movie {ImdbId}", imdbId);
                failedCount++;
            }
        }

        _logger.LogInformation("Movie seed completed. Imported: {Imported}, Failed: {Failed}",
            importedCount, failedCount);
    }
}
