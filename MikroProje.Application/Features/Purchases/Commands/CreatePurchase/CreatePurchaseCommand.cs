using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Purchases.DTOs;

namespace MikroProje.Application.Features.Purchases.Commands.CreatePurchase;

public class CreatePurchaseCommand : IRequest<Result<PurchaseDto>>
{
    public int CurrentAccountId { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public string? Description { get; set; }

    public List<CreatePurchaseItemRequest> Items { get; set; } = new();
}
