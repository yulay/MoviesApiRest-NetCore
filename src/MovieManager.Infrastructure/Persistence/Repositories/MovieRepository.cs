using MongoDB.Bson;
using MongoDB.Driver;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Interfaces;
using MovieManager.Infrastructure.Persistence.Context;

namespace MovieManager.Infrastructure.Persistence.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly MongoDbContext _context;

    public MovieRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Movie?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _context.Movies
            .Find(m => m.Id == id && !m.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<Movie?> GetByExternalIdAsync(string externalId)
    {
        return await _context.Movies
            .Find(m => m.ExternalId == externalId && !m.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(int page, int pageSize)
    {
        return await _context.Movies
            .Find(m => !m.IsDeleted)
            .SortByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Movie>> SearchByTitleAsync(string title, int page, int pageSize)
    {
        var filter = Builders<Movie>.Filter.And(
            Builders<Movie>.Filter.Regex(m => m.Title, new BsonRegularExpression(title, "i")),
            Builders<Movie>.Filter.Eq(m => m.IsDeleted, false)
        );

        return await _context.Movies
            .Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Movie>> GetByGenreAsync(string genre, int page, int pageSize)
    {
        var filter = Builders<Movie>.Filter.And(
            Builders<Movie>.Filter.AnyEq(m => m.Genres, genre),
            Builders<Movie>.Filter.Eq(m => m.IsDeleted, false)
        );

        return await _context.Movies
            .Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Movie>> GetByDirectorAsync(string director, int page, int pageSize)
    {
        var filter = Builders<Movie>.Filter.And(
            Builders<Movie>.Filter.Regex(m => m.Director, new BsonRegularExpression(director, "i")),
            Builders<Movie>.Filter.Eq(m => m.IsDeleted, false)
        );

        return await _context.Movies
            .Find(filter)
            .SortByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<Movie?> GetRandomAsync()
    {
        var count = await _context.Movies.CountDocumentsAsync(m => !m.IsDeleted);
        if (count == 0) return null;

        var random = new Random();
        var skip = random.Next(0, (int)count);

        return await _context.Movies
            .Find(m => !m.IsDeleted)
            .Skip(skip)
            .Limit(1)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Movie>> GetRecommendationsByGenreAsync(string genre, int count)
    {
        var filter = Builders<Movie>.Filter.And(
            Builders<Movie>.Filter.AnyEq(m => m.Genres, genre),
            Builders<Movie>.Filter.Eq(m => m.IsDeleted, false)
        );

        return await _context.Movies
            .Find(filter)
            .SortByDescending(m => m.Rating)
            .Limit(count)
            .ToListAsync();
    }

    public async Task<long> GetTotalCountAsync()
    {
        return await _context.Movies.CountDocumentsAsync(m => !m.IsDeleted);
    }

    public async Task<Dictionary<string, int>> GetCountByGenreAsync()
    {
        var movies = await _context.Movies
            .Find(m => !m.IsDeleted)
            .ToListAsync();

        return movies
            .SelectMany(m => m.Genres)
            .GroupBy(g => g)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<int, int>> GetCountByYearAsync()
    {
        var movies = await _context.Movies
            .Find(m => !m.IsDeleted)
            .ToListAsync();

        return movies
            .GroupBy(m => m.Year)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetTopDirectorsAsync(int count)
    {
        var movies = await _context.Movies
            .Find(m => !m.IsDeleted && !string.IsNullOrEmpty(m.Director))
            .ToListAsync();

        return movies
            .GroupBy(m => m.Director)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Movie> CreateAsync(Movie movie)
    {
        movie.Id = ObjectId.GenerateNewId().ToString();
        await _context.Movies.InsertOneAsync(movie);
        return movie;
    }

    public async Task<Movie> UpdateAsync(Movie movie)
    {
        await _context.Movies.ReplaceOneAsync(m => m.Id == movie.Id, movie);
        return movie;
    }

    public async Task<bool> SoftDeleteAsync(string id)
    {
        var update = Builders<Movie>.Update
            .Set(m => m.IsDeleted, true)
            .Set(m => m.UpdatedAt, DateTime.UtcNow);

        var result = await _context.Movies.UpdateOneAsync(m => m.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return false;

        return await _context.Movies
            .Find(m => m.Id == id && !m.IsDeleted)
            .AnyAsync();
    }
}
