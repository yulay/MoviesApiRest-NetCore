using MediatR;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Statistics.Queries;

public record GetTotalMoviesQuery : IRequest<long>;

public class GetTotalMoviesQueryHandler : IRequestHandler<GetTotalMoviesQuery, long>
{
    private readonly IMovieRepository _movieRepository;

    public GetTotalMoviesQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<long> Handle(GetTotalMoviesQuery request, CancellationToken cancellationToken)
    {
        return await _movieRepository.GetTotalCountAsync();
    }
}
