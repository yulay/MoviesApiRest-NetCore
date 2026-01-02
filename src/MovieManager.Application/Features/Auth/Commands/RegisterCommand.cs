using MediatR;
using MovieManager.Application.DTOs.Auth;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.Interfaces;
using MovieManager.Domain.Entities;
using MovieManager.Domain.Enums;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Auth.Commands;

public record RegisterCommand(RegisterDto Register) : IRequest<ResultDto<TokenDto>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ResultDto<TokenDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<ResultDto<TokenDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.ExistsAsync(request.Register.Email);

        if (existingUser)
        {
            return ResultDto<TokenDto>.FailureResult("El correo electrónico ya está registrado");
        }

        var user = new User
        {
            Email = request.Register.Email.ToLower(),
            PasswordHash = _passwordHasher.HashPassword(request.Register.Password),
            FirstName = request.Register.FirstName,
            LastName = request.Register.LastName,
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            RefreshToken = _jwtService.GenerateRefreshToken(),
            RefreshTokenExpiry = _jwtService.GetRefreshTokenExpiry()
        };

        var createdUser = await _userRepository.CreateAsync(user);

        var tokenDto = new TokenDto
        {
            AccessToken = _jwtService.GenerateAccessToken(createdUser),
            RefreshToken = createdUser.RefreshToken!,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Email = createdUser.Email,
            Role = createdUser.Role.ToString()
        };

        return ResultDto<TokenDto>.SuccessResult(tokenDto, "Usuario registrado exitosamente");
    }
}
