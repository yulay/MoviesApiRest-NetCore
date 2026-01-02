using FluentAssertions;
using Moq;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Application.Features.Movies.Commands;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Tests.Application.Handlers;

public class CreateMovieCommandHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly CreateMovieCommandHandler _handler;

    public CreateMovieCommandHandlerTests()
    {
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _handler = new CreateMovieCommandHandler(_movieRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidMovie_ReturnsSuccessResult()
    {
        var createDto = new MovieCreateDto
        {
            Title = "Test Movie",
            Description = "Test Description",
            Year = 2024,
            Genres = new List<string> { "Action", "Drama" },
            Director = "Test Director",
            Actors = new List<string> { "Actor 1", "Actor 2" },
            Rating = 8.5m,
            Duration = 120,
            Poster = "http://example.com/poster.jpg"
        };

        var command = new CreateMovieCommand(createDto);

        _movieRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Movie>()))
            .ReturnsAsync((Movie m) =>
            {
                m.Id = "generated-id-123";
                return m;
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Title.Should().Be("Test Movie");
        result.Data.Year.Should().Be(2024);
        result.Data.Id.Should().Be("generated-id-123");

        _movieRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Movie>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SetsCreatedAtToUtcNow()
    {
        var createDto = new MovieCreateDto
        {
            Title = "Test Movie",
            Year = 2024,
            Genres = new List<string>(),
            Actors = new List<string>()
        };

        var command = new CreateMovieCommand(createDto);
        Movie? capturedMovie = null;

        _movieRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Movie>()))
            .Callback<Movie>(m => capturedMovie = m)
            .ReturnsAsync((Movie m) => m);

        var beforeCall = DateTime.UtcNow;
        await _handler.Handle(command, CancellationToken.None);
        var afterCall = DateTime.UtcNow;

        capturedMovie.Should().NotBeNull();
        capturedMovie!.CreatedAt.Should().BeOnOrAfter(beforeCall);
        capturedMovie.CreatedAt.Should().BeOnOrBefore(afterCall);
        capturedMovie.IsDeleted.Should().BeFalse();
    }
}
