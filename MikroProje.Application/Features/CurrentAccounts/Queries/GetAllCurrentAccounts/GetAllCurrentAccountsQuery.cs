using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.DTOs;

namespace MikroProje.Application.Features.CurrentAccounts.Queries.GetAllCurrentAccounts;

public class GetAllCurrentAccountsQuery : IRequest<Result<IReadOnlyCollection<CurrentAccountDto>>>
{
}
