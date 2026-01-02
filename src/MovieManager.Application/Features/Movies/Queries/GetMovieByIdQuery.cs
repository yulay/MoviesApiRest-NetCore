using MediatR;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Movies.Queries;

public record GetMovieByIdQuery(string Id) : IRequest<ResultDto<MovieDto>>;

public class GetMovieByIdQueryHandler : IRequestHandler<GetMovieByIdQuery, ResultDto<MovieDto>>
{
    private readonly IMovieRepository _movieRepository;

    public GetMovieByIdQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<ResultDto<MovieDto>> Handle(GetMovieByIdQuery request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(request.Id);

        if (movie == null || movie.IsDeleted)
        {
            return ResultDto<MovieDto>.FailureResult("Película no encontrada");
        }

        var movieDto = new MovieDto
        {
            Id = movie.Id,
            ExternalId = movie.ExternalId,
            Title = movie.Title,
            Description = movie.Description,
            Year = movie.Year,
            Genres = movie.Genres,
            Director = movie.Director,
            Actors = movie.Actors,
            Rating = movie.Rating,
            Duration = movie.Duration,
            Poster = movie.Poster,
            CreatedAt = movie.CreatedAt,
            UpdatedAt = movie.UpdatedAt
        };

        return ResultDto<MovieDto>.SuccessResult(movieDto);
    }
}
