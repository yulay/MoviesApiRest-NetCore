using MediatR;
using MovieManager.Application.DTOs.Statistics;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Statistics.Queries;

public record GetTopDirectorsQuery(int Count = 10) : IRequest<IEnumerable<DirectorStatDto>>;

public class GetTopDirectorsQueryHandler : IRequestHandler<GetTopDirectorsQuery, IEnumerable<DirectorStatDto>>
{
    private readonly IMovieRepository _movieRepository;

    public GetTopDirectorsQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<DirectorStatDto>> Handle(GetTopDirectorsQuery request, CancellationToken cancellationToken)
    {
        var stats = await _movieRepository.GetTopDirectorsAsync(request.Count);

        return stats.Select(s => new DirectorStatDto
        {
            Director = s.Key,
            Count = s.Value
        }).OrderByDescending(s => s.Count);
    }
}
