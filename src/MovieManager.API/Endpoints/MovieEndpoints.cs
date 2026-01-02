using MediatR;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Application.Features.Movies.Commands;
using MovieManager.Application.Features.Movies.Queries;

namespace MovieManager.API.Endpoints;

public static class MovieEndpoints
{
    public static void MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/movies")
            .WithTags("Películas")
            .RequireAuthorization("AllAuthenticated");

        group.MapGet("/", async (int page, int pageSize, IMediator mediator) =>
        {
            var query = new GetMoviesQuery(page > 0 ? page : 1, pageSize > 0 ? pageSize : 10);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetMovies")
        .WithDescription("Obtener lista paginada de películas");

        group.MapGet("/{id}", async (string id, IMediator mediator) =>
        {
            var query = new GetMovieByIdQuery(id);
            var result = await mediator.Send(query);

            if (result == null)
                return Results.NotFound(new { Message = "Película no encontrada" });

            return Results.Ok(result);
        })
        .WithName("GetMovieById")
        .WithDescription("Obtener una película por su ID");

        group.MapPost("/", async (MovieCreateDto request, IMediator mediator) =>
        {
            var command = new CreateMovieCommand(request);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Created($"/api/movies/{result.Data?.Id}", result);
        })
        .WithName("CreateMovie")
        .WithDescription("Crear una nueva película")
        .RequireAuthorization("AdminOnly");

        group.MapPut("/{id}", async (string id, MovieUpdateDto request, IMediator mediator) =>
        {
            var command = new UpdateMovieCommand(id, request);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Ok(result);
        })
        .WithName("UpdateMovie")
        .WithDescription("Actualizar una película existente")
        .RequireAuthorization("EditorOrAdmin");

        group.MapDelete("/{id}", async (string id, IMediator mediator) =>
        {
            var command = new DeleteMovieCommand(id);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.NotFound(result);

            return Results.Ok(result);
        })
        .WithName("DeleteMovie")
        .WithDescription("Eliminar una película (soft delete)")
        .RequireAuthorization("AdminOnly");

        group.MapGet("/search", async (string title, int page, int pageSize, IMediator mediator) =>
        {
            var query = new SearchMoviesQuery(title, page > 0 ? page : 1, pageSize > 0 ? pageSize : 10);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("SearchMovies")
        .WithDescription("Buscar películas por título");

        group.MapGet("/genre/{genre}", async (string genre, int page, int pageSize, IMediator mediator) =>
        {
            var query = new GetMoviesByGenreQuery(genre, page > 0 ? page : 1, pageSize > 0 ? pageSize : 10);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetMoviesByGenre")
        .WithDescription("Obtener películas por género");

        group.MapGet("/director/{director}", async (string director, int page, int pageSize, IMediator mediator) =>
        {
            var query = new GetMoviesByDirectorQuery(director, page > 0 ? page : 1, pageSize > 0 ? pageSize : 10);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetMoviesByDirector")
        .WithDescription("Obtener películas por director");

        group.MapGet("/random", async (IMediator mediator) =>
        {
            var query = new GetRandomMovieQuery();
            var result = await mediator.Send(query);

            if (result == null)
                return Results.NotFound(new { Message = "No hay películas disponibles" });

            return Results.Ok(result);
        })
        .WithName("GetRandomMovie")
        .WithDescription("Obtener una película aleatoria");

        group.MapGet("/recommendations/{genre}", async (string genre, int count, IMediator mediator) =>
        {
            var query = new GetRecommendationsQuery(genre, count > 0 ? count : 5);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetRecommendations")
        .WithDescription("Obtener recomendaciones de películas por género");
    }
}
