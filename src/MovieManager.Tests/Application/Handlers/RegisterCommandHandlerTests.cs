using FluentAssertions;
using Moq;
using MovieManager.Application.DTOs.Auth;
using MovieManager.Application.Features.Auth.Commands;
using MovieManager.Application.Interfaces;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Tests.Application.Handlers;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtServiceMock = new Mock<IJwtService>();
        _handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_NewUser_ReturnsSuccessWithToken()
    {
        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "Password123!",
            FirstName = "New",
            LastName = "User"
        };

        _userRepositoryMock
            .Setup(x => x.ExistsAsync("newuser@example.com"))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword("Password123!"))
            .Returns("hashed-password");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token-123");

        _jwtServiceMock
            .Setup(x => x.GetRefreshTokenExpiry())
            .Returns(DateTime.UtcNow.AddDays(7));

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) =>
            {
                u.Id = "new-user-id";
                return u;
            });

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("access-token-123");

        var command = new RegisterCommand(registerDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("access-token-123");
        result.Data.RefreshToken.Should().Be("refresh-token-123");
        result.Data.Email.Should().Be("newuser@example.com");
    }

    [Fact]
    public async Task Handle_ExistingEmail_ReturnsFailure()
    {
        var registerDto = new RegisterDto
        {
            Email = "existing@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        _userRepositoryMock
            .Setup(x => x.ExistsAsync("existing@example.com"))
            .ReturnsAsync(true);

        var command = new RegisterCommand(registerDto);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ya está registrado"));

        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NewUser_SetsDefaultRoleAsUser()
    {
        var registerDto = new RegisterDto
        {
            Email = "newuser@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };

        User? capturedUser = null;

        _userRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns("hashed");

        _jwtServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh");

        _jwtServiceMock
            .Setup(x => x.GetRefreshTokenExpiry())
            .Returns(DateTime.UtcNow.AddDays(7));

        _userRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => u);

        _jwtServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("token");

        var command = new RegisterCommand(registerDto);

        await _handler.Handle(command, CancellationToken.None);

        capturedUser.Should().NotBeNull();
        capturedUser!.Role.Should().Be(Domain.Enums.UserRole.User);
        capturedUser.IsActive.Should().BeTrue();
    }
}
