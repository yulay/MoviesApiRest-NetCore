using MediatR;
using MovieManager.Application.DTOs.Common;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Movies.Commands;

public record DeleteMovieCommand(string Id) : IRequest<ResultDto<bool>>;

public class DeleteMovieCommandHandler : IRequestHandler<DeleteMovieCommand, ResultDto<bool>>
{
    private readonly IMovieRepository _movieRepository;

    public DeleteMovieCommandHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<ResultDto<bool>> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(request.Id);

        if (movie == null || movie.IsDeleted)
        {
            return ResultDto<bool>.FailureResult("Película no encontrada");
        }

        var result = await _movieRepository.SoftDeleteAsync(request.Id);

        if (result)
        {
            return ResultDto<bool>.SuccessResult(true, "Película eliminada exitosamente");
        }

        return ResultDto<bool>.FailureResult("Error al eliminar la película");
    }
}
