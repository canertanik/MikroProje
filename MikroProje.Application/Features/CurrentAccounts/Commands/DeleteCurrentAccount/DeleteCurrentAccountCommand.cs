using MediatR;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.CurrentAccounts.Commands.DeleteCurrentAccount;

public class DeleteCurrentAccountCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
