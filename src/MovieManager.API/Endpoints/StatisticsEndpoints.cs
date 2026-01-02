using MediatR;
using MovieManager.Application.DTOs.Statistics;
using MovieManager.Application.Features.Statistics.Queries;

namespace MovieManager.API.Endpoints;

public static class StatisticsEndpoints
{
    public static void MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/statistics")
            .WithTags("Estadísticas")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/total-movies", async (IMediator mediator) =>
        {
            var query = new GetTotalMoviesQuery();
            var result = await mediator.Send(query);
            return Results.Ok(new { TotalMovies = result });
        })
        .WithName("GetTotalMovies")
        .WithSummary("Total de películas")
        .WithDescription("Retorna el número total de películas activas en la base de datos. **Solo Admin**.")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/genres", async (IMediator mediator) =>
        {
            var query = new GetGenreStatsQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetGenreStats")
        .WithSummary("Estadísticas por género")
        .WithDescription("Retorna la cantidad de películas por cada género. **Solo Admin**.")
        .Produces<List<GenreStatDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/years-distribution", async (IMediator mediator) =>
        {
            var query = new GetYearStatsQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetYearStats")
        .WithSummary("Distribución por año")
        .WithDescription("Retorna la cantidad de películas por año de lanzamiento. **Solo Admin**.")
        .Produces<List<YearStatDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/top-directors", async (int count, IMediator mediator) =>
        {
            var query = new GetTopDirectorsQuery(count > 0 ? count : 10);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetTopDirectors")
        .WithSummary("Directores destacados")
        .WithDescription("Retorna los directores con mayor cantidad de películas en la colección. **Solo Admin**.")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "Cantidad de directores a retornar (default: 10)";
            return operation;
        })
        .Produces<List<DirectorStatDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
