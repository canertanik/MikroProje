using MediatR;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.Purchases.Commands.CancelPurchase;

public class CancelPurchaseCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
