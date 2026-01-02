using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using MovieManager.Infrastructure.Services;

namespace MovieManager.Tests.Infrastructure;

public class CacheServiceTests
{
    private readonly CacheService _cacheService;
    private readonly IMemoryCache _memoryCache;

    public CacheServiceTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _cacheService = new CacheService(_memoryCache);
    }

    [Fact]
    public void Set_And_Get_ReturnsStoredValue()
    {
        var key = "test-key";
        var value = "test-value";

        _cacheService.Set(key, value);
        var result = _cacheService.Get<string>(key);

        result.Should().Be(value);
    }

    [Fact]
    public void Get_NonExistentKey_ReturnsDefault()
    {
        var result = _cacheService.Get<string>("non-existent-key");

        result.Should().BeNull();
    }

    [Fact]
    public void Set_ComplexObject_ReturnsCorrectly()
    {
        var key = "complex-key";
        var value = new TestObject { Id = 1, Name = "Test" };

        _cacheService.Set(key, value);
        var result = _cacheService.Get<TestObject>(key);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public void Remove_ExistingKey_RemovesValue()
    {
        var key = "remove-key";
        var value = "test-value";

        _cacheService.Set(key, value);
        _cacheService.Remove(key);
        var result = _cacheService.Get<string>(key);

        result.Should().BeNull();
    }

    [Fact]
    public void TryGetValue_ExistingKey_ReturnsTrueAndValue()
    {
        var key = "try-get-key";
        var value = "test-value";

        _cacheService.Set(key, value);
        var success = _cacheService.TryGetValue<string>(key, out var result);

        success.Should().BeTrue();
        result.Should().Be(value);
    }

    [Fact]
    public void TryGetValue_NonExistentKey_ReturnsFalse()
    {
        var success = _cacheService.TryGetValue<string>("non-existent", out var result);

        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public async Task Set_WithExpiration_ExpiresCorrectly()
    {
        var key = "expiring-key";
        var value = "test-value";
        var expiration = TimeSpan.FromMilliseconds(100);

        _cacheService.Set(key, value, expiration);

        var immediateResult = _cacheService.Get<string>(key);
        immediateResult.Should().Be(value);

        await Task.Delay(150);

        var expiredResult = _cacheService.Get<string>(key);
        expiredResult.Should().BeNull();
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
