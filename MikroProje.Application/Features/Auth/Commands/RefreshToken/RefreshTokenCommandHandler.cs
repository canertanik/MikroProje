using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Auth.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAppUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IAppUserRepository userRepository,
        IConfiguration configuration)
    {
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.Request.AccessToken);
        if (principal == null)
        {
            return Result<AuthResponseDto>.Fail("Geçersiz access token.", 401);
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Result<AuthResponseDto>.Fail("Geçersiz access token içeriği.", 401);
        }

        var tokenHash = _tokenService.HashRefreshToken(request.Request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken == null || existingToken.UserId != userId)
        {
            return Result<AuthResponseDto>.Fail("Geçersiz refresh token.", 401);
        }

        if (existingToken.IsRevoked)
        {
            // Token Reuse Attempt: Kötü niyetli biri iptal edilmiş token kullanmaya çalıştı.
            // Ekstra güvenlik: Tüm tokenları revoke etme işlemi de eklenebilir, şimdilik sadece reddediyoruz.
            return Result<AuthResponseDto>.Fail("Bu refresh token iptal edilmiş.", 401);
        }

        if (existingToken.IsExpired)
        {
            return Result<AuthResponseDto>.Fail("Refresh token süresi dolmuş. Lütfen tekrar giriş yapın.", 401);
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Fail("Kullanıcı bulunamadı veya pasif durumda.", 401);
        }

        // Rotate Token (Eski token iptal edilir, yenisi üretilir)
        var newRefreshTokenStr = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashRefreshToken(newRefreshTokenStr);

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByTokenHash = newRefreshTokenHash;

        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        var newRefreshToken = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays)
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var accessTokenMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        var response = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            AccessTokenExpiration = DateTime.UtcNow.AddMinutes(accessTokenMinutes),
            RefreshToken = newRefreshTokenStr,
            RefreshTokenExpiration = newRefreshToken.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role
            }
        };

        return Result<AuthResponseDto>.Ok(response, "Token başarıyla yenilendi.");
    }
}
