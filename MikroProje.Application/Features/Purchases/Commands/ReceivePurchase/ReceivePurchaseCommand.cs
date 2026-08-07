using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Purchases.DTOs;

namespace MikroProje.Application.Features.Purchases.Commands.ReceivePurchase;

public class ReceivePurchaseCommand : IRequest<Result<PurchaseDto>>
{
    public int Id { get; set; }
}
