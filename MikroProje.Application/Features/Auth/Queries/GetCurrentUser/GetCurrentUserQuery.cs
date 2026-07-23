using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Auth.DTOs;

namespace MikroProje.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<Result<UserDto>>
{
    public int UserId { get; set; }
}
