using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MovieManager.API.HealthChecks;

/// <summary>
/// Health check para verificar la disponibilidad de la API de OMDb
/// </summary>
public class OmdbHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public OmdbHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var baseUrl = _configuration["Omdb:BaseUrl"];
            var apiKey = _configuration["Omdb:ApiKey"];

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey))
            {
                return HealthCheckResult.Unhealthy("Configuración de OMDb no encontrada");
            }

            // Hacer una petición simple para verificar que la API responde
            var response = await client.GetAsync(
                $"{baseUrl}?apikey={apiKey}&t=test",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("OMDb API disponible");
            }

            return HealthCheckResult.Degraded($"OMDb API respondió con código: {response.StatusCode}");
        }
        catch (HttpRequestException ex)
        {
            return HealthCheckResult.Unhealthy("No se puede conectar a OMDb API", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error al verificar OMDb API", ex);
        }
    }
}
