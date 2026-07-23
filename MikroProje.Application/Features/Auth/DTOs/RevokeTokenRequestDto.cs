namespace MikroProje.Application.Features.Auth.DTOs;

public class RevokeTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
