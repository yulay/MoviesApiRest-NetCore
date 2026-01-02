using FluentAssertions;
using Moq;
using MovieManager.Application.Features.Movies.Commands;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Tests.Application.Handlers;

public class DeleteMovieCommandHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly DeleteMovieCommandHandler _handler;

    public DeleteMovieCommandHandlerTests()
    {
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _handler = new DeleteMovieCommandHandler(_movieRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingMovie_ReturnsSuccessResult()
    {
        var movieId = "507f1f77bcf86cd799439011";
        var movie = new Movie
        {
            Id = movieId,
            Title = "Test Movie",
            IsDeleted = false,
            Genres = new List<string>(),
            Actors = new List<string>()
        };

        _movieRepositoryMock
            .Setup(x => x.GetByIdAsync(movieId))
            .ReturnsAsync(movie);

        _movieRepositoryMock
            .Setup(x => x.SoftDeleteAsync(movieId))
            .ReturnsAsync(true);

        var command = new DeleteMovieCommand(movieId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("eliminada");

        _movieRepositoryMock.Verify(x => x.SoftDeleteAsync(movieId), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingMovie_ReturnsFailureResult()
    {
        var movieId = "nonexistent-id";

        _movieRepositoryMock
            .Setup(x => x.GetByIdAsync(movieId))
            .ReturnsAsync((Movie?)null);

        var command = new DeleteMovieCommand(movieId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("no encontrada"));

        _movieRepositoryMock.Verify(x => x.SoftDeleteAsync(It.IsAny<string>()), Times.Never);
    }
}
