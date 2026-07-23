using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Auth.DTOs;

namespace MikroProje.Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<Result<UserDto>>
{
    public RegisterRequestDto Request { get; set; } = null!;
}
