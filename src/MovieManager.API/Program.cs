using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MovieManager.API.Endpoints;
using MovieManager.API.HealthChecks;
using MovieManager.API.Middleware;
using MovieManager.Application;
using MovieManager.Infrastructure;
using MovieManager.Infrastructure.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EditorOrAdmin", policy => policy.RequireRole("Editor", "Admin"));
    options.AddPolicy("AllAuthenticated", policy => policy.RequireAuthenticatedUser());
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Movie Manager API",
        Version = "v1",
        Description = @"
## API REST para Gestión de Películas

Esta API permite gestionar una colección de películas con las siguientes características:

### Funcionalidades principales:
- **CRUD completo** de películas con soft delete
- **Integración con OMDb** para importar y sincronizar datos
- **Autenticación JWT** con refresh tokens
- **Sistema de roles**: Admin, Editor, User
- **Búsqueda y filtrado** por título, género y director
- **Estadísticas** de la colección
- **Rate limiting**: 100 requests/minuto

### Roles y permisos:
| Rol | Permisos |
|-----|----------|
| **Admin** | CRUD completo + Estadísticas + Importación OMDb |
| **Editor** | Edición de películas (PUT) |
| **User** | Solo lectura (GET) |

### Usuario de prueba:
- **Email**: admin@moviemanager.com
- **Password**: Admin123!
",
        Contact = new OpenApiContact
        {
            Name = "Movie Manager Team",
            Email = "soporte@moviemanager.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = @"
Autenticación JWT Bearer.

Para obtener un token:
1. Use el endpoint POST /api/auth/login con sus credenciales
2. Copie el accessToken de la respuesta
3. Haga clic en 'Authorize' e ingrese el token

**Ejemplo**: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Ordenar tags alfabéticamente
    options.OrderActionsBy(api => api.GroupName);
});

// Health Checks
builder.Services.AddHttpClient();
builder.Services.AddHealthChecks()
    .AddCheck<MongoDbHealthCheck>(
        "mongodb",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "mongodb" })
    .AddCheck<OmdbHealthCheck>(
        "omdb-api",
        failureStatus: HealthStatus.Degraded,
        tags: new[] { "external", "omdb" });

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Movie Manager API v1");
    options.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapMovieEndpoints();
app.MapStatisticsEndpoints();
app.MapIntegrationEndpoints();

// Health Check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                tags = e.Value.Tags
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
})
.WithTags("Health")
.WithSummary("Estado de salud del sistema")
.WithDescription("Retorna el estado de salud de la API y sus dependencias (MongoDB, OMDb)")
.AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            database = report.Entries.FirstOrDefault().Value.Status.ToString()
        }));
    }
})
.WithTags("Health")
.WithSummary("Verificar disponibilidad")
.WithDescription("Verifica si la API está lista para recibir tráfico (conexión a base de datos)")
.AllowAnonymous();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
})
.WithTags("Health")
.WithSummary("Verificar que la API está viva")
.WithDescription("Endpoint básico para verificar que la aplicación responde")
.AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<IDataSeederService>();
    await seeder.SeedAsync();
}

app.Run();

public partial class Program { }