using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Enums;
using MovieManager.Infrastructure.Services;
using MovieManager.Infrastructure.Settings;

namespace MovieManager.Tests.Infrastructure;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly JwtSettings _settings;

    public JwtServiceTests()
    {
        _settings = new JwtSettings
        {
            SecretKey = "TestSecretKeyThatIsLongEnoughForHmacSha256Algorithm!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };

        var options = Options.Create(_settings);
        _jwtService = new JwtService(options);
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwt()
    {
        var user = new User
        {
            Id = "user-id-123",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Admin
        };

        var token = _jwtService.GenerateAccessToken(user);

        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Should().NotBeNull();
        // Verificar que el token tiene el formato correcto (3 partes separadas por .)
        token.Split('.').Should().HaveCount(3);
        // Verificar audience
        jwtToken.Audiences.Should().Contain(_settings.Audience);
        // Verificar que contiene el claim sub con el Id del usuario
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserClaims()
    {
        var user = new User
        {
            Id = "user-id-123",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.Editor
        };

        var token = _jwtService.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Verificar claims principales que están disponibles al leer el token
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id);
        jwtToken.Claims.Should().Contain(c => c.Value == "Editor");
        jwtToken.Claims.Should().Contain(c => c.Type == "lastName" && c.Value == user.LastName);
        // El token debe tener más de 3 claims (sub, role, lastName, exp, aud mínimo)
        jwtToken.Claims.Count().Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public void GenerateAccessToken_HasCorrectExpiration()
    {
        var user = new User
        {
            Id = "user-id-123",
            Email = "test@example.com",
            Role = UserRole.User
        };

        var beforeGeneration = DateTime.UtcNow;
        var token = _jwtService.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiry = beforeGeneration.AddMinutes(_settings.ExpirationMinutes);

        // JWT timestamps pierden precisión de milisegundos, usar tolerancia de 2 segundos
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64String()
    {
        var refreshToken = _jwtService.GenerateRefreshToken();

        refreshToken.Should().NotBeNullOrEmpty();

        var action = () => Convert.FromBase64String(refreshToken);
        action.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDifferentTokensEachTime()
    {
        var token1 = _jwtService.GenerateRefreshToken();
        var token2 = _jwtService.GenerateRefreshToken();

        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GetRefreshTokenExpiry_ReturnsCorrectDate()
    {
        var beforeCall = DateTime.UtcNow;
        var expiry = _jwtService.GetRefreshTokenExpiry();
        var afterCall = DateTime.UtcNow;

        var expectedMinExpiry = beforeCall.AddDays(_settings.RefreshTokenExpirationDays);
        var expectedMaxExpiry = afterCall.AddDays(_settings.RefreshTokenExpirationDays);

        expiry.Should().BeOnOrAfter(expectedMinExpiry);
        expiry.Should().BeOnOrBefore(expectedMaxExpiry);
    }
}
