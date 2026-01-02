using MediatR;
using MovieManager.Application.DTOs.Integration;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Integration.Queries;

public record SearchOmdbQuery(string Title) : IRequest<IEnumerable<OmdbSearchResultDto>>;

public class SearchOmdbQueryHandler : IRequestHandler<SearchOmdbQuery, IEnumerable<OmdbSearchResultDto>>
{
    private readonly IOmdbService _omdbService;

    public SearchOmdbQueryHandler(IOmdbService omdbService)
    {
        _omdbService = omdbService;
    }

    public async Task<IEnumerable<OmdbSearchResultDto>> Handle(SearchOmdbQuery request, CancellationToken cancellationToken)
    {
        var results = await _omdbService.SearchByTitleAsync(request.Title);

        return results.Select(r => new OmdbSearchResultDto
        {
            ImdbId = r.ImdbId,
            Title = r.Title,
            Year = r.Year,
            Type = r.Type,
            Poster = r.Poster
        });
    }
}
