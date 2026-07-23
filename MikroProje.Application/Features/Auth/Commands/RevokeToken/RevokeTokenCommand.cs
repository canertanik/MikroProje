using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Auth.DTOs;

namespace MikroProje.Application.Features.Auth.Commands.RevokeToken;

public class RevokeTokenCommand : IRequest<Result<bool>>
{
    public RevokeTokenRequestDto Request { get; set; } = null!;
}
