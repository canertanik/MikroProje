using MediatR;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Auth.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IAppUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
        IAppUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Request.Email, cancellationToken);

        // Same error message for user not found and invalid password to prevent email enumeration
        if (user == null || !_passwordHasher.VerifyPassword(request.Request.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Fail("Geçersiz e-posta veya parola.", 401);
        }

        if (!user.IsActive)
        {
            return Result<AuthResponseDto>.Fail("Hesabınız pasife alınmış.", 401);
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenStr = _tokenService.GenerateRefreshToken();
        var refreshTokenHash = _tokenService.HashRefreshToken(refreshTokenStr);
        
        var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        var refreshToken = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        var accessTokenMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(accessTokenMinutes),
            RefreshToken = refreshTokenStr,
            RefreshTokenExpiration = refreshToken.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role
            }
        };

        return Result<AuthResponseDto>.Ok(response, "Giriş başarılı.");
    }
}
