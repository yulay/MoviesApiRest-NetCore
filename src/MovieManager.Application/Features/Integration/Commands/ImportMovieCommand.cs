using MediatR;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Integration.Commands;

public record ImportMovieCommand(string ImdbId) : IRequest<ResultDto<MovieDto>>;

public class ImportMovieCommandHandler : IRequestHandler<ImportMovieCommand, ResultDto<MovieDto>>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IOmdbService _omdbService;

    public ImportMovieCommandHandler(IMovieRepository movieRepository, IOmdbService omdbService)
    {
        _movieRepository = movieRepository;
        _omdbService = omdbService;
    }

    public async Task<ResultDto<MovieDto>> Handle(ImportMovieCommand request, CancellationToken cancellationToken)
    {
        var existingMovie = await _movieRepository.GetByExternalIdAsync(request.ImdbId);

        if (existingMovie != null && !existingMovie.IsDeleted)
        {
            return ResultDto<MovieDto>.FailureResult("La película ya existe en la base de datos");
        }

        var movie = await _omdbService.GetByImdbIdAsync(request.ImdbId);

        if (movie == null)
        {
            return ResultDto<MovieDto>.FailureResult("Película no encontrada en OMDb");
        }

        movie.CreatedAt = DateTime.UtcNow;
        movie.IsDeleted = false;

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

        return ResultDto<MovieDto>.SuccessResult(movieDto, "Película importada exitosamente desde OMDb");
    }
}
