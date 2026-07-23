using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.DTOs;

namespace MikroProje.Application.Features.CurrentAccounts.Queries.GetCurrentAccountById;

public class GetCurrentAccountByIdQuery : IRequest<Result<CurrentAccountDto>>
{
    public int Id { get; set; }
}
