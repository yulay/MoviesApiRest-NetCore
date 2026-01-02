using MediatR;
using MovieManager.Application.DTOs.Statistics;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Statistics.Queries;

public record GetYearStatsQuery : IRequest<IEnumerable<YearStatDto>>;

public class GetYearStatsQueryHandler : IRequestHandler<GetYearStatsQuery, IEnumerable<YearStatDto>>
{
    private readonly IMovieRepository _movieRepository;

    public GetYearStatsQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<YearStatDto>> Handle(GetYearStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _movieRepository.GetCountByYearAsync();

        return stats.Select(s => new YearStatDto
        {
            Year = s.Key,
            Count = s.Value
        }).OrderByDescending(s => s.Year);
    }
}
