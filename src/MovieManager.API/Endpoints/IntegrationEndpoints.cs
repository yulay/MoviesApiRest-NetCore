using MediatR;
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
        .WithDescription("Buscar películas en OMDb");

        group.MapPost("/import/{externalId}", async (string externalId, IMediator mediator) =>
        {
            var command = new ImportMovieCommand(externalId);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Created($"/api/movies/{result.Data?.Id}", result);
        })
        .WithName("ImportMovie")
        .WithDescription("Importar película desde OMDb por IMDb ID");

        group.MapPut("/sync/{id}", async (string id, IMediator mediator) =>
        {
            var command = new SyncMovieCommand(id);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Ok(result);
        })
        .WithName("SyncMovie")
        .WithDescription("Sincronizar película existente con datos de OMDb");
    }
}
