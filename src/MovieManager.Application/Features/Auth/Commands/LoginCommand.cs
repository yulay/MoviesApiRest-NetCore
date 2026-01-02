using MediatR;
using MovieManager.Application.DTOs.Auth;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.Interfaces;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Auth.Commands;

public record LoginCommand(LoginDto Login) : IRequest<ResultDto<TokenDto>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ResultDto<TokenDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<ResultDto<TokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Login.Email.ToLower());

        if (user == null)
        {
            return ResultDto<TokenDto>.FailureResult("Credenciales inválidas");
        }

        if (!user.IsActive)
        {
            return ResultDto<TokenDto>.FailureResult("Usuario desactivado");
        }

        if (!_passwordHasher.VerifyPassword(request.Login.Password, user.PasswordHash))
        {
            return ResultDto<TokenDto>.FailureResult("Credenciales inválidas");
        }

        user.RefreshToken = _jwtService.GenerateRefreshToken();
        user.RefreshTokenExpiry = _jwtService.GetRefreshTokenExpiry();
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        var tokenDto = new TokenDto
        {
            AccessToken = _jwtService.GenerateAccessToken(user),
            RefreshToken = user.RefreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Email = user.Email,
            Role = user.Role.ToString()
        };

        return ResultDto<TokenDto>.SuccessResult(tokenDto, "Inicio de sesión exitoso");
    }
}
