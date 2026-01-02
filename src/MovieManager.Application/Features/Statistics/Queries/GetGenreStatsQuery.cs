using MediatR;
using MovieManager.Application.DTOs.Statistics;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Statistics.Queries;

public record GetGenreStatsQuery : IRequest<IEnumerable<GenreStatDto>>;

public class GetGenreStatsQueryHandler : IRequestHandler<GetGenreStatsQuery, IEnumerable<GenreStatDto>>
{
    private readonly IMovieRepository _movieRepository;

    public GetGenreStatsQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<GenreStatDto>> Handle(GetGenreStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _movieRepository.GetCountByGenreAsync();

        return stats.Select(s => new GenreStatDto
        {
            Genre = s.Key,
            Count = s.Value
        }).OrderByDescending(s => s.Count);
    }
}
