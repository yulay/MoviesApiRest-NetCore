using MongoDB.Bson;
using MongoDB.Driver;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Interfaces;
using MovieManager.Infrastructure.Persistence.Context;

namespace MovieManager.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MongoDbContext _context;

    public UserRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        if (!ObjectId.TryParse(id, out _))
            return null;

        return await _context.Users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Find(u => u.Email.ToLower() == email.ToLower())
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users
            .Find(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        user.Id = ObjectId.GenerateNewId().ToString();
        await _context.Users.InsertOneAsync(user);
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
        return user;
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Users
            .Find(u => u.Email.ToLower() == email.ToLower())
            .AnyAsync();
    }
}
