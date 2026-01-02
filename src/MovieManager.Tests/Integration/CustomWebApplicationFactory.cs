using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Interfaces;
using MovieManager.Infrastructure.Services;

namespace MovieManager.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IMovieRepository> MovieRepositoryMock { get; private set; } = new();
    public Mock<IUserRepository> UserRepositoryMock { get; private set; } = new();
    public Mock<IOmdbService> OmdbServiceMock { get; private set; } = new();

    public void ResetMocks()
    {
        MovieRepositoryMock = new Mock<IMovieRepository>();
        UserRepositoryMock = new Mock<IUserRepository>();
        OmdbServiceMock = new Mock<IOmdbService>();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(IMovieRepository) ||
                           d.ServiceType == typeof(IUserRepository) ||
                           d.ServiceType == typeof(IOmdbService) ||
                           d.ServiceType == typeof(IDataSeederService))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddScoped(_ => MovieRepositoryMock.Object);
            services.AddScoped(_ => UserRepositoryMock.Object);
            services.AddScoped(_ => OmdbServiceMock.Object);
            services.AddScoped<IDataSeederService, NoOpDataSeederService>();
        });

        builder.UseEnvironment("Testing");
    }
}

public class NoOpDataSeederService : IDataSeederService
{
    public Task SeedAsync() => Task.CompletedTask;
}
