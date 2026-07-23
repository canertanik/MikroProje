using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Auth.DTOs;

namespace MikroProje.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<Result<AuthResponseDto>>
{
    public LoginRequestDto Request { get; set; } = null!;
}
