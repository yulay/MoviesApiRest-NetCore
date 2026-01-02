using MediatR;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Movies.Queries;

public record GetMoviesByGenreQuery(string Genre, int Page = 1, int PageSize = 10) : IRequest<PaginatedResultDto<MovieDto>>;

public class GetMoviesByGenreQueryHandler : IRequestHandler<GetMoviesByGenreQuery, PaginatedResultDto<MovieDto>>
{
    private readonly IMovieRepository _movieRepository;

    public GetMoviesByGenreQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<PaginatedResultDto<MovieDto>> Handle(GetMoviesByGenreQuery request, CancellationToken cancellationToken)
    {
        var movies = await _movieRepository.GetByGenreAsync(request.Genre, request.Page, request.PageSize);

        var movieDtos = movies.Select(movie => new MovieDto
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
        });

        return new PaginatedResultDto<MovieDto>
        {
            Items = movieDtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = movieDtos.Count()
        };
    }
}
