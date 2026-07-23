using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Purchases.DTOs;

namespace MikroProje.Application.Features.Purchases.Queries.GetPurchaseById;

public class GetPurchaseByIdQuery : IRequest<Result<PurchaseDto>>
{
    public int Id { get; set; }
}
