using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MovieManager.Domain.Entities;
using MovieManager.Infrastructure.Settings;

namespace MovieManager.Infrastructure.Persistence.Context;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);

        CreateIndexes();
    }

    public IMongoCollection<Movie> Movies => _database.GetCollection<Movie>(_settings.MoviesCollectionName);
    public IMongoCollection<User> Users => _database.GetCollection<User>(_settings.UsersCollectionName);

    private void CreateIndexes()
    {
        var movieIndexes = new List<CreateIndexModel<Movie>>
        {
            new(Builders<Movie>.IndexKeys.Text(m => m.Title)),
            new(Builders<Movie>.IndexKeys.Ascending(m => m.ExternalId)),
            new(Builders<Movie>.IndexKeys.Ascending(m => m.IsDeleted)),
            new(Builders<Movie>.IndexKeys.Ascending(m => m.Director)),
            new(Builders<Movie>.IndexKeys.Ascending(m => m.Year))
        };

        Movies.Indexes.CreateMany(movieIndexes);

        var userIndexes = new List<CreateIndexModel<User>>
        {
            new(Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true }),
            new(Builders<User>.IndexKeys.Ascending(u => u.RefreshToken))
        };

        Users.Indexes.CreateMany(userIndexes);
    }
}
