using MediatR;
using MovieManager.Application.DTOs.Auth;
using MovieManager.Application.DTOs.Common;
using MovieManager.Application.Interfaces;
using MovieManager.Domain.Interfaces;

namespace MovieManager.Application.Features.Auth.Commands;

public record RefreshTokenCommand(RefreshTokenDto RefreshToken) : IRequest<ResultDto<TokenDto>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ResultDto<TokenDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<ResultDto<TokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken.RefreshToken);

        if (user == null)
        {
            return ResultDto<TokenDto>.FailureResult("Token de refresco inválido");
        }

        if (!user.IsActive)
        {
            return ResultDto<TokenDto>.FailureResult("Usuario desactivado");
        }

        if (user.RefreshTokenExpiry < DateTime.UtcNow)
        {
            return ResultDto<TokenDto>.FailureResult("Token de refresco expirado");
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

        return ResultDto<TokenDto>.SuccessResult(tokenDto, "Token renovado exitosamente");
    }
}
