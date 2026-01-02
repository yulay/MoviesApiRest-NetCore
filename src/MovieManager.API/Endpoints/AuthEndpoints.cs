using MediatR;
using MovieManager.Application.DTOs.Auth;
using MovieManager.Application.Features.Auth.Commands;

namespace MovieManager.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Autenticación");

        group.MapPost("/register", async (RegisterDto request, IMediator mediator) =>
        {
            var command = new RegisterCommand(request);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Ok(result);
        })
        .WithName("Register")
        .WithDescription("Registrar un nuevo usuario")
        .Produces<Application.DTOs.Common.ResultDto<TokenDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/login", async (LoginDto request, IMediator mediator) =>
        {
            var command = new LoginCommand(request);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Ok(result);
        })
        .WithName("Login")
        .WithDescription("Iniciar sesión")
        .Produces<Application.DTOs.Common.ResultDto<TokenDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/refresh-token", async (RefreshTokenDto request, IMediator mediator) =>
        {
            var command = new RefreshTokenCommand(request);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Ok(result);
        })
        .WithName("RefreshToken")
        .WithDescription("Renovar token de acceso")
        .Produces<Application.DTOs.Common.ResultDto<TokenDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
