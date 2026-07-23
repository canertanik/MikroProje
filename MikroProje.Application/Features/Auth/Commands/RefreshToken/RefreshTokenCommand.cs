using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Auth.DTOs;

namespace MikroProje.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<Result<AuthResponseDto>>
{
    public RefreshTokenRequestDto Request { get; set; } = null!;
}
