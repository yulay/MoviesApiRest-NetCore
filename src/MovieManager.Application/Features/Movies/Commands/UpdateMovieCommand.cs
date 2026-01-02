using MediatR;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Movies.Commands;

public record UpdateMovieCommand(string Id, MovieUpdateDto Movie) : IRequest<ResultDto<MovieDto>>;

public class UpdateMovieCommandHandler : IRequestHandler<UpdateMovieCommand, ResultDto<MovieDto>>
{
    private readonly IMovieRepository _movieRepository;

    public UpdateMovieCommandHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<ResultDto<MovieDto>> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var existingMovie = await _movieRepository.GetByIdAsync(request.Id);

        if (existingMovie == null || existingMovie.IsDeleted)
        {
            return ResultDto<MovieDto>.FailureResult("Película no encontrada");
        }

        existingMovie.Title = request.Movie.Title;
        existingMovie.Description = request.Movie.Description;
        existingMovie.Year = request.Movie.Year;
        existingMovie.Genres = request.Movie.Genres;
        existingMovie.Director = request.Movie.Director;
        existingMovie.Actors = request.Movie.Actors;
        existingMovie.Rating = request.Movie.Rating;
        existingMovie.Duration = request.Movie.Duration;
        existingMovie.Poster = request.Movie.Poster;
        existingMovie.UpdatedAt = DateTime.UtcNow;

        var updatedMovie = await _movieRepository.UpdateAsync(existingMovie);

        var movieDto = new MovieDto
        {
            Id = updatedMovie.Id,
            ExternalId = updatedMovie.ExternalId,
            Title = updatedMovie.Title,
            Description = updatedMovie.Description,
            Year = updatedMovie.Year,
            Genres = updatedMovie.Genres,
            Director = updatedMovie.Director,
            Actors = updatedMovie.Actors,
            Rating = updatedMovie.Rating,
            Duration = updatedMovie.Duration,
            Poster = updatedMovie.Poster,
            CreatedAt = updatedMovie.CreatedAt,
            UpdatedAt = updatedMovie.UpdatedAt
        };

        return ResultDto<MovieDto>.SuccessResult(movieDto, "Película actualizada exitosamente");
    }
}
