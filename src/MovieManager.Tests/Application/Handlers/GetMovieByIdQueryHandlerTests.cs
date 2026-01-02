using FluentAssertions;
using Moq;
using MovieManager.Application.Features.Movies.Queries;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Tests.Application.Handlers;

public class GetMovieByIdQueryHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly GetMovieByIdQueryHandler _handler;

    public GetMovieByIdQueryHandlerTests()
    {
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _handler = new GetMovieByIdQueryHandler(_movieRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMovie_ReturnsSuccessWithMovieDto()
    {
        var movieId = "507f1f77bcf86cd799439011";
        var movie = new Movie
        {
            Id = movieId,
            Title = "The Shawshank Redemption",
            Description = "A great movie",
            Year = 1994,
            Genres = new List<string> { "Drama" },
            Director = "Frank Darabont",
            Actors = new List<string> { "Tim Robbins", "Morgan Freeman" },
            Rating = 9.3m,
            Duration = 142,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _movieRepositoryMock
            .Setup(x => x.GetByIdAsync(movieId))
            .ReturnsAsync(movie);

        var query = new GetMovieByIdQuery(movieId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(movieId);
        result.Data.Title.Should().Be("The Shawshank Redemption");
        result.Data.Year.Should().Be(1994);
        result.Data.Director.Should().Be("Frank Darabont");
    }

    [Fact]
    public async Task Handle_NonExistingMovie_ReturnsFailure()
    {
        var movieId = "nonexistent-id";

        _movieRepositoryMock
            .Setup(x => x.GetByIdAsync(movieId))
            .ReturnsAsync((Movie?)null);

        var query = new GetMovieByIdQuery(movieId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.Errors.Should().Contain(e => e.Contains("no encontrada"));
    }

    [Fact]
    public async Task Handle_DeletedMovie_ReturnsFailure()
    {
        var movieId = "507f1f77bcf86cd799439011";
        var movie = new Movie
        {
            Id = movieId,
            Title = "Deleted Movie",
            IsDeleted = true,
            Genres = new List<string>(),
            Actors = new List<string>()
        };

        _movieRepositoryMock
            .Setup(x => x.GetByIdAsync(movieId))
            .ReturnsAsync(movie);

        var query = new GetMovieByIdQuery(movieId);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("no encontrada"));
    }
}
