using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using MovieManager.Application.DTOs.Auth;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Enums;

namespace MovieManager.Tests.Integration;

public class MovieEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public MovieEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> GetAuthTokenAsync(HttpClient client, UserRole role = UserRole.Admin)
    {
        var email = $"user-{Guid.NewGuid()}@test.com";
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            FirstName = "Test",
            LastName = "User",
            Role = role,
            IsActive = true
        };

        _factory.UserRepositoryMock
            .Setup(x => x.GetByEmailAsync(email))
            .ReturnsAsync(user);

        _factory.UserRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = email,
            Password = "Password123!"
        });

        var result = await loginResponse.Content.ReadFromJsonAsync<ResultDto<TokenDto>>();
        return result?.Data?.AccessToken ?? string.Empty;
    }

    [Fact]
    public async Task GetMovies_WithoutAuth_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/movies?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMovies_WithAuth_ReturnsOk()
    {
        using var client = _factory.CreateClient();
        var token = await GetAuthTokenAsync(client, UserRole.User);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var movies = new List<Movie>
        {
            new Movie { Id = "1", Title = "Movie 1", Year = 2024, Genres = new List<string>(), Actors = new List<string>() },
            new Movie { Id = "2", Title = "Movie 2", Year = 2023, Genres = new List<string>(), Actors = new List<string>() }
        };

        _factory.MovieRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(movies);

        var response = await client.GetAsync("/api/movies?page=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMovieById_ExistingMovie_ReturnsOk()
    {
        using var client = _factory.CreateClient();
        var token = await GetAuthTokenAsync(client, UserRole.User);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var movieId = "507f1f77bcf86cd799439011";
        var movie = new Movie
        {
            Id = movieId,
            Title = "Test Movie",
            Year = 2024,
            Genres = new List<string> { "Action" },
            Actors = new List<string> { "Actor 1" },
            Director = "Director",
            Rating = 8.5m,
            IsDeleted = false
        };

        _factory.MovieRepositoryMock
            .Setup(x => x.GetByIdAsync(movieId))
            .ReturnsAsync(movie);

        var response = await client.GetAsync($"/api/movies/{movieId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ResultDto<MovieDto>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data!.Title.Should().Be("Test Movie");
    }

    [Fact]
    public async Task CreateMovie_AsAdmin_ReturnsCreated()
    {
        using var client = _factory.CreateClient();
        var token = await GetAuthTokenAsync(client, UserRole.Admin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        _factory.MovieRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Movie>()))
            .ReturnsAsync((Movie m) =>
            {
                m.Id = "new-movie-id";
                return m;
            });

        var createDto = new MovieCreateDto
        {
            Title = "New Movie",
            Description = "Description",
            Year = 2024,
            Genres = new List<string> { "Action" },
            Actors = new List<string> { "Actor" },
            Director = "Director",
            Rating = 8.0m,
            Duration = 120
        };

        var response = await client.PostAsJsonAsync("/api/movies", createDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateMovie_AsUser_ReturnsForbidden()
    {
        using var client = _factory.CreateClient();
        var token = await GetAuthTokenAsync(client, UserRole.User);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createDto = new MovieCreateDto
        {
            Title = "New Movie",
            Year = 2024,
            Genres = new List<string>(),
            Actors = new List<string>()
        };

        var response = await client.PostAsJsonAsync("/api/movies", createDto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteMovie_AsAdmin_ReturnsOk()
    {
        using var client = _factory.CreateClient();
        var token = await GetAuthTokenAsync(client, UserRole.Admin);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var movieId = "test-movie-id";
        var movie = new Movie
        {
            Id = movieId,
            Title = "Test Movie",
            Year = 2024,
            Genres = new List<string>(),
            Actors = new List<string>(),
            IsDeleted = false
        };

        _factory.MovieRepositoryMock
            .Setup(x => x.GetByIdAsync(movieId))
            .ReturnsAsync(movie);

        _factory.MovieRepositoryMock
            .Setup(x => x.SoftDeleteAsync(movieId))
            .ReturnsAsync(true);

        var response = await client.DeleteAsync($"/api/movies/{movieId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
