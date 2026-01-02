using MediatR;
using MovieManager.Application.DTOs.Auth;
using MovieManager.Application.DTOs.Common;
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
        .WithSummary("Registrar nuevo usuario")
        .WithDescription(@"
Crea una nueva cuenta de usuario con rol 'User' por defecto.

**Requisitos de contraseña:**
- Mínimo 6 caracteres
- Al menos una mayúscula
- Al menos una minúscula
- Al menos un número

**Respuesta exitosa:** Retorna tokens de acceso y refresh.")
        .Produces<ResultDto<TokenDto>>(StatusCodes.Status200OK)
        .Produces<ResultDto<TokenDto>>(StatusCodes.Status400BadRequest)
        .AllowAnonymous();

        group.MapPost("/login", async (LoginDto request, IMediator mediator) =>
        {
            var command = new LoginCommand(request);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Ok(result);
        })
        .WithName("Login")
        .WithSummary("Iniciar sesión")
        .WithDescription(@"
Autentica un usuario y retorna tokens JWT.

**Usuario de prueba (Admin):**
- Email: admin@moviemanager.com
- Password: Admin123!

**Tokens retornados:**
- `accessToken`: Token JWT para autenticación (válido por 60 minutos)
- `refreshToken`: Token para renovar el access token (válido por 7 días)")
        .Produces<ResultDto<TokenDto>>(StatusCodes.Status200OK)
        .Produces<ResultDto<TokenDto>>(StatusCodes.Status400BadRequest)
        .AllowAnonymous();

        group.MapPost("/refresh-token", async (RefreshTokenDto request, IMediator mediator) =>
        {
            var command = new RefreshTokenCommand(request);
            var result = await mediator.Send(command);

            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Ok(result);
        })
        .WithName("RefreshToken")
        .WithSummary("Renovar token de acceso")
        .WithDescription(@"
Genera un nuevo par de tokens usando el refresh token actual.

**Uso:** Cuando el access token expire, use este endpoint para obtener nuevos tokens sin necesidad de volver a iniciar sesión.")
        .Produces<ResultDto<TokenDto>>(StatusCodes.Status200OK)
        .Produces<ResultDto<TokenDto>>(StatusCodes.Status400BadRequest)
        .AllowAnonymous();
    }
}
