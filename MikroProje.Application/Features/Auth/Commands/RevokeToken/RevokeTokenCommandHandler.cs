using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result<bool>>
{
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public RevokeTokenCommandHandler(ITokenService tokenService, IRefreshTokenRepository refreshTokenRepository)
    {
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result<bool>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.Request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken == null)
        {
            return Result<bool>.Fail("Refresh token bulunamadı.", 404);
        }

        if (existingToken.IsRevoked)
        {
            return Result<bool>.Fail("Refresh token zaten iptal edilmiş.", 400);
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        // API design: 200 OK or 204 NoContent. Result pattern normally handles 200 or 204 gracefully.
        return Result<bool>.Ok(true, "Token başarıyla iptal edildi.");
    }
}
