using System.Security.Claims;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(AppUser user);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
