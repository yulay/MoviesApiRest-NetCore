using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MovieManager.API.HealthChecks;

/// <summary>
/// Health check para verificar la conexión a MongoDB
/// </summary>
public class MongoDbHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public MongoDbHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration["MongoDb:ConnectionString"];
            var databaseName = _configuration["MongoDb:DatabaseName"];

            if (string.IsNullOrEmpty(connectionString))
            {
                return HealthCheckResult.Unhealthy("Connection string de MongoDB no configurada");
            }

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName ?? "MoviesDb");

            // Ejecutar un comando simple para verificar la conexión
            await database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1),
                cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("Conexión a MongoDB establecida");
        }
        catch (MongoException ex)
        {
            return HealthCheckResult.Unhealthy("Error de conexión a MongoDB", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error al verificar MongoDB", ex);
        }
    }
}
