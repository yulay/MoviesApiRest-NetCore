using FluentAssertions;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Application.Validators;

namespace MovieManager.Tests.Application.Validators;

public class MovieCreateValidatorTests
{
    private readonly MovieCreateValidator _validator;

    public MovieCreateValidatorTests()
    {
        _validator = new MovieCreateValidator();
    }

    [Fact]
    public void Validate_ValidMovie_ReturnsNoErrors()
    {
        var movie = new MovieCreateDto
        {
            Title = "Test Movie",
            Description = "A test movie description",
            Year = 2024,
            Genres = new List<string> { "Action" },
            Director = "Test Director",
            Actors = new List<string> { "Actor 1" },
            Rating = 8.5m,
            Duration = 120,
            Poster = "http://example.com/poster.jpg"
        };

        var result = _validator.Validate(movie);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyTitle_ReturnsError()
    {
        var movie = new MovieCreateDto
        {
            Title = "",
            Year = 2024,
            Genres = new List<string> { "Action" },
            Actors = new List<string>()
        };

        var result = _validator.Validate(movie);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_TitleTooLong_ReturnsError()
    {
        var movie = new MovieCreateDto
        {
            Title = new string('A', 201),
            Year = 2024,
            Genres = new List<string> { "Action" },
            Actors = new List<string>()
        };

        var result = _validator.Validate(movie);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Theory]
    [InlineData(1800)]
    [InlineData(2100)]
    public void Validate_InvalidYear_ReturnsError(int year)
    {
        var movie = new MovieCreateDto
        {
            Title = "Test Movie",
            Year = year,
            Genres = new List<string> { "Action" },
            Actors = new List<string>()
        };

        var result = _validator.Validate(movie);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Year");
    }

    [Fact]
    public void Validate_ValidYear_ReturnsNoErrors()
    {
        var movie = new MovieCreateDto
        {
            Title = "Test Movie",
            Year = 2024,
            Genres = new List<string> { "Action" },
            Actors = new List<string>()
        };

        var result = _validator.Validate(movie);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Validate_InvalidRating_ReturnsError(decimal rating)
    {
        var movie = new MovieCreateDto
        {
            Title = "Test Movie",
            Year = 2024,
            Genres = new List<string> { "Action" },
            Actors = new List<string>(),
            Rating = rating
        };

        var result = _validator.Validate(movie);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Rating");
    }

    [Fact]
    public void Validate_NegativeDuration_ReturnsError()
    {
        var movie = new MovieCreateDto
        {
            Title = "Test Movie",
            Year = 2024,
            Genres = new List<string> { "Action" },
            Actors = new List<string>(),
            Duration = -10
        };

        var result = _validator.Validate(movie);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Duration");
    }
}
