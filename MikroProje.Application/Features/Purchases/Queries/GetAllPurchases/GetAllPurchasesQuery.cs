using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Purchases.DTOs;

namespace MikroProje.Application.Features.Purchases.Queries.GetAllPurchases;

public class GetAllPurchasesQuery : IRequest<Result<PagedResult<PurchaseListDto>>>
{
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public MikroProje.Domain.Enums.PurchaseStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
