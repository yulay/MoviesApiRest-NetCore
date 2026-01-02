using FluentAssertions;
using Moq;
using MovieManager.Application.DTOs.Auth;
using MovieManager.Application.Features.Auth.Commands;
using MovieManager.Application.Interfaces;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Enums;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Tests.Application.Handlers;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokenDto()
    {
        var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
        var user = new User
        {
            Id = "user-id-123",
            Email = "test@example.com",
            PasswordHash = "hashed-password",
            FirstName = "Test",
            LastName = "User",
            Role = UserRole.User,
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("Password123!", "hashed-password"))
            .Returns(true);

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("access-token-123");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token-123");

        _jwtServiceMock
            .Setup(x => x.GetRefreshTokenExpiry())
            .Returns(DateTime.UtcNow.AddDays(7));

        _userRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        var command = new LoginCommand(loginDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("access-token-123");
        result.Data.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsFailure()
    {
        var loginDto = new LoginDto { Email = "nonexistent@example.com", Password = "Password123!" };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("nonexistent@example.com"))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand(loginDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("inválidas"));
    }

    [Fact]
    public async Task Handle_InvalidPassword_ReturnsFailure()
    {
        var loginDto = new LoginDto { Email = "test@example.com", Password = "WrongPassword" };
        var user = new User
        {
            Id = "user-id-123",
            Email = "test@example.com",
            PasswordHash = "hashed-password",
            IsActive = true
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.VerifyPassword("WrongPassword", "hashed-password"))
            .Returns(false);

        var command = new LoginCommand(loginDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("inválidas"));
    }

    [Fact]
    public async Task Handle_InactiveUser_ReturnsFailure()
    {
        var loginDto = new LoginDto { Email = "test@example.com", Password = "Password123!" };
        var user = new User
        {
            Id = "user-id-123",
            Email = "test@example.com",
            PasswordHash = "hashed-password",
            IsActive = false
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        var command = new LoginCommand(loginDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("desactivado"));
    }
}
