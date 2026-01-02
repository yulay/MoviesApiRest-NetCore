using MediatR;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Movies.Commands;

public record CreateMovieCommand(MovieCreateDto Movie) : IRequest<ResultDto<MovieDto>>;

public class CreateMovieCommandHandler : IRequestHandler<CreateMovieCommand, ResultDto<MovieDto>>
{
    private readonly IMovieRepository _movieRepository;

    public CreateMovieCommandHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<ResultDto<MovieDto>> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = new Movie
        {
            Title = request.Movie.Title,
            Description = request.Movie.Description,
            Year = request.Movie.Year,
            Genres = request.Movie.Genres,
            Director = request.Movie.Director,
            Actors = request.Movie.Actors,
            Rating = request.Movie.Rating,
            Duration = request.Movie.Duration,
            Poster = request.Movie.Poster,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var createdMovie = await _movieRepository.CreateAsync(movie);

        var movieDto = new MovieDto
        {
            Id = createdMovie.Id,
            ExternalId = createdMovie.ExternalId,
            Title = createdMovie.Title,
            Description = createdMovie.Description,
            Year = createdMovie.Year,
            Genres = createdMovie.Genres,
            Director = createdMovie.Director,
            Actors = createdMovie.Actors,
            Rating = createdMovie.Rating,
            Duration = createdMovie.Duration,
            Poster = createdMovie.Poster,
            CreatedAt = createdMovie.CreatedAt,
            UpdatedAt = createdMovie.UpdatedAt
        };

        return ResultDto<MovieDto>.SuccessResult(movieDto, "Película creada exitosamente");
    }
}
