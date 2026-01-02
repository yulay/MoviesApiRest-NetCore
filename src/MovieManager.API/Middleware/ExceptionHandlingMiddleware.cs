using System.Net;
using System.Text.Json;
using FluentValidation;

namespace MovieManager.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Error de validación";
                errorResponse.Errors = validationEx.Errors
                    .Select(e => e.ErrorMessage)
                    .ToList();
                _logger.LogWarning("Validation error: {Errors}", string.Join(", ", errorResponse.Errors));
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "No autorizado";
                _logger.LogWarning("Unauthorized access attempt");
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = "Recurso no encontrado";
                _logger.LogWarning("Resource not found: {Message}", exception.Message);
                break;

            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = argEx.Message;
                _logger.LogWarning("Argument error: {Message}", argEx.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = "Ha ocurrido un error interno";
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteAsync(result);
    }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
}
