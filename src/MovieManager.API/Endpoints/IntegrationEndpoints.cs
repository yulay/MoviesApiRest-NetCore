using MediatR;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.DTOs.Integration;
using MovieManager.Application.DTOs.Movies;
using MovieManager.Application.Features.Integration.Commands;
using MovieManager.Application.Features.Integration.Queries;

namespace MovieManager.API.Endpoints;

public static class IntegrationEndpoints
{
    public static void MapIntegrationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/integration")
            .WithTags("Integración OMDb")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/search-external", async (string title, IMediator mediator) =>
        {
            var query = new SearchOmdbQuery(title);
            var result = await mediator.Send(query);
            return Results.Ok(result);
        })
        .WithName("SearchExternal")
        .WithSummary("Buscar en OMDb")
        .WithDescription(@"
Busca películas en la API externa de OMDb (Open Movie Database).

**Uso:** Busque películas por título para obtener su IMDb ID, luego use el endpoint de importación para agregarlas a la base de datos local.

**Solo Admin**.")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "Título de la película a buscar";
            return operation;
        })
        .Produces<ResultDto<List<OmdbSearchResultDto>>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/import/{externalId}", async (string externalId, IMediator mediator) =>
        {
            var command = new ImportMovieCommand(externalId);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Created($"/api/movies/{result.Data?.Id}", result);
        })
        .WithName("ImportMovie")
        .WithSummary("Importar desde OMDb")
        .WithDescription(@"
Importa una película desde OMDb usando su IMDb ID.

**Pasos:**
1. Use `/search-external` para encontrar la película
2. Copie el `imdbId` del resultado
3. Use este endpoint para importar la película

**Solo Admin**.")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "IMDb ID de la película (ej: tt0111161)";
            return operation;
        })
        .Produces<ResultDto<MovieDto>>(StatusCodes.Status201Created)
        .Produces<ResultDto<MovieDto>>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/sync/{id}", async (string id, IMediator mediator) =>
        {
            var command = new SyncMovieCommand(id);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Ok(result);
        })
        .WithName("SyncMovie")
        .WithSummary("Sincronizar con OMDb")
        .WithDescription(@"
Actualiza una película existente con los datos más recientes de OMDb.

**Requisito:** La película debe tener un `externalId` (IMDb ID) asociado.

**Solo Admin**.")
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "ID interno de la película en MongoDB";
            return operation;
        })
        .Produces<ResultDto<MovieDto>>(StatusCodes.Status200OK)
        .Produces<ResultDto<MovieDto>>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    }
}
