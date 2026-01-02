using MediatR;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Movies.Queries;

public record GetRandomMovieQuery : IRequest<ResultDto<MovieDto>>;

public class GetRandomMovieQueryHandler : IRequestHandler<GetRandomMovieQuery, ResultDto<MovieDto>>
{
    private readonly IMovieRepository _movieRepository;

    public GetRandomMovieQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<ResultDto<MovieDto>> Handle(GetRandomMovieQuery request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetRandomAsync();

        if (movie == null)
        {
            return ResultDto<MovieDto>.FailureResult("No hay películas disponibles");
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
