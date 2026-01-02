using MediatR;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Movies.Queries;

public record GetRecommendationsQuery(string Genre, int Count = 5) : IRequest<IEnumerable<MovieDto>>;

public class GetRecommendationsQueryHandler : IRequestHandler<GetRecommendationsQuery, IEnumerable<MovieDto>>
{
    private readonly IMovieRepository _movieRepository;

    public GetRecommendationsQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<MovieDto>> Handle(GetRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var movies = await _movieRepository.GetRecommendationsByGenreAsync(request.Genre, request.Count);

        return movies.Select(movie => new MovieDto
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
    }
}
