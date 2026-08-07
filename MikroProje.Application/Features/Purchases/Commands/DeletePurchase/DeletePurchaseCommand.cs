using MediatR;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.Purchases.Commands.DeletePurchase;

public class DeletePurchaseCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
