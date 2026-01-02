using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MovieManager.Application.Interfaces;
using MovieManager.Domain.Interfaces;
using MovieManager.Infrastructure.External;
using MovieManager.Infrastructure.Persistence.Context;
using MovieManager.Infrastructure.Persistence.Repositories;
using MovieManager.Infrastructure.Services;
using MovieManager.Infrastructure.Settings;

namespace MovieManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDb"));
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<OmdbSettings>(configuration.GetSection("Omdb"));

        services.AddSingleton<MongoDbContext>();

        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddHttpClient<IOmdbService, OmdbService>();

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();

        services.AddScoped<IDataSeederService, DataSeederService>();

        return services;
    }
}
