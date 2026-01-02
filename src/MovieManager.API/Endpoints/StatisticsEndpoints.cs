using MediatR;
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
        .WithDescription("Obtener el total de películas");

        group.MapGet("/genres", async (IMediator mediator) =>
        {
            var query = new GetGenreStatsQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetGenreStats")
        .WithDescription("Obtener estadísticas por género");

        group.MapGet("/years-distribution", async (IMediator mediator) =>
        {
            var query = new GetYearStatsQuery();
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetYearStats")
        .WithDescription("Obtener distribución de películas por año");

        group.MapGet("/top-directors", async (int count, IMediator mediator) =>
        {
            var query = new GetTopDirectorsQuery(count > 0 ? count : 10);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetTopDirectors")
        .WithDescription("Obtener los directores con más películas");
    }
}
