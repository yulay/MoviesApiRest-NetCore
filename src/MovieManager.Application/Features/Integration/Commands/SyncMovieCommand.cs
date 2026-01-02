using MediatR;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Integration.Commands;

public record SyncMovieCommand(string Id) : IRequest<ResultDto<MovieDto>>;

public class SyncMovieCommandHandler : IRequestHandler<SyncMovieCommand, ResultDto<MovieDto>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IOmdbService _omdbService;

    public SyncMovieCommandHandler(IMovieRepository movieRepository, IOmdbService omdbService)
    {
        _movieRepository = movieRepository;
        _omdbService = omdbService;
    }

    public async Task<ResultDto<MovieDto>> Handle(SyncMovieCommand request, CancellationToken cancellationToken)
    {
        var existingMovie = await _movieRepository.GetByIdAsync(request.Id);

        if (existingMovie == null || existingMovie.IsDeleted)
        {
            return ResultDto<MovieDto>.FailureResult("Película no encontrada");
        }

        if (string.IsNullOrEmpty(existingMovie.ExternalId))
        {
            return ResultDto<MovieDto>.FailureResult("La película no tiene un ID externo para sincronizar");
        }

        var omdbMovie = await _omdbService.GetByImdbIdAsync(existingMovie.ExternalId);

        if (omdbMovie == null)
        {
            return ResultDto<MovieDto>.FailureResult("No se pudo obtener información de OMDb");
        }

        existingMovie.Title = omdbMovie.Title;
        existingMovie.Description = omdbMovie.Description;
        existingMovie.Year = omdbMovie.Year;
        existingMovie.Genres = omdbMovie.Genres;
        existingMovie.Director = omdbMovie.Director;
        existingMovie.Actors = omdbMovie.Actors;
        existingMovie.Rating = omdbMovie.Rating;
        existingMovie.Duration = omdbMovie.Duration;
        existingMovie.Poster = omdbMovie.Poster;
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

        return ResultDto<MovieDto>.SuccessResult(movieDto, "Película sincronizada exitosamente con OMDb");
    }
}
