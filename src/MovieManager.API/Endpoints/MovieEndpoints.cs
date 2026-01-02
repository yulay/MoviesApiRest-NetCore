using MediatR;
using MovieManager.Application.DTOs.Common;
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
        .WithSummary("Obtener películas paginadas")
        .WithDescription("Retorna una lista paginada de todas las películas activas. Requiere autenticación.")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "Número de página (default: 1)";
            operation.Parameters[1].Description = "Cantidad de elementos por página (default: 10, max: 50)";
            return operation;
        })
        .Produces<PaginatedResultDto<MovieDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id}", async (string id, IMediator mediator) =>
        {
            var query = new GetMovieByIdQuery(id);
            var result = await mediator.Send(query);

            if (!result.Success)
                return Results.NotFound(result);

            return Results.Ok(result);
        })
        .WithName("GetMovieById")
        .WithSummary("Obtener película por ID")
        .WithDescription("Retorna los detalles completos de una película específica.")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "ID único de la película (MongoDB ObjectId)";
            return operation;
        })
        .Produces<ResultDto<MovieDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/", async (MovieCreateDto request, IMediator mediator) =>
        {
            var command = new CreateMovieCommand(request);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Created($"/api/movies/{result.Data?.Id}", result);
        })
        .WithName("CreateMovie")
        .WithSummary("Crear nueva película")
        .WithDescription("Crea una nueva película en la base de datos. **Solo Admin**.")
        .Produces<ResultDto<MovieDto>>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
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
        .WithSummary("Actualizar película")
        .WithDescription("Actualiza los datos de una película existente. **Editor o Admin**.")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "ID único de la película a actualizar";
            return operation;
        })
        .Produces<ResultDto<MovieDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
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
        .WithSummary("Eliminar película")
        .WithDescription("Realiza un soft delete de la película (marca IsDeleted=true). **Solo Admin**.")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "ID único de la película a eliminar";
            return operation;
        })
        .Produces<ResultDto<bool>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .RequireAuthorization("AdminOnly");

        group.MapGet("/search", async (string title, int page, int pageSize, IMediator mediator) =>
        {
            var query = new SearchMoviesQuery(title, page > 0 ? page : 1, pageSize > 0 ? pageSize : 10);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("SearchMovies")
        .WithSummary("Buscar películas por título")
        .WithDescription("Busca películas cuyo título contenga el texto especificado (case-insensitive).")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "Texto a buscar en el título";
            operation.Parameters[1].Description = "Número de página (default: 1)";
            operation.Parameters[2].Description = "Cantidad por página (default: 10)";
            return operation;
        })
        .Produces<PaginatedResultDto<MovieDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/genre/{genre}", async (string genre, int page, int pageSize, IMediator mediator) =>
        {
            var query = new GetMoviesByGenreQuery(genre, page > 0 ? page : 1, pageSize > 0 ? pageSize : 10);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetMoviesByGenre")
        .WithSummary("Filtrar por género")
        .WithDescription("Obtiene películas que pertenecen al género especificado.")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "Género a filtrar (ej: Action, Drama, Comedy)";
            return operation;
        })
        .Produces<PaginatedResultDto<MovieDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/director/{director}", async (string director, int page, int pageSize, IMediator mediator) =>
        {
            var query = new GetMoviesByDirectorQuery(director, page > 0 ? page : 1, pageSize > 0 ? pageSize : 10);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetMoviesByDirector")
        .WithSummary("Filtrar por director")
        .WithDescription("Obtiene películas dirigidas por el director especificado.")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "Nombre del director";
            return operation;
        })
        .Produces<PaginatedResultDto<MovieDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/random", async (IMediator mediator) =>
        {
            var query = new GetRandomMovieQuery();
            var result = await mediator.Send(query);

            if (!result.Success)
                return Results.NotFound(result);

            return Results.Ok(result);
        })
        .WithName("GetRandomMovie")
        .WithSummary("Película aleatoria")
        .WithDescription("Retorna una película seleccionada aleatoriamente de la colección.")
        .Produces<ResultDto<MovieDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/recommendations/{genre}", async (string genre, int count, IMediator mediator) =>
        {
            var query = new GetRecommendationsQuery(genre, count > 0 ? count : 5);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetRecommendations")
        .WithSummary("Recomendaciones por género")
        .WithDescription("Obtiene películas recomendadas basadas en el género especificado.")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "Género para las recomendaciones";
            operation.Parameters[1].Description = "Cantidad de recomendaciones (default: 5)";
            return operation;
        })
        .Produces<ResultDto<List<MovieDto>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}
