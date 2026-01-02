using MediatR;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Movies.Queries;

public record SearchMoviesQuery(string Title, int Page = 1, int PageSize = 10) : IRequest<PaginatedResultDto<MovieDto>>;

public class SearchMoviesQueryHandler : IRequestHandler<SearchMoviesQuery, PaginatedResultDto<MovieDto>>
{
    private readonly IMovieRepository _movieRepository;

    public SearchMoviesQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<PaginatedResultDto<MovieDto>> Handle(SearchMoviesQuery request, CancellationToken cancellationToken)
    {
        var movies = await _movieRepository.SearchByTitleAsync(request.Title, request.Page, request.PageSize);

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
