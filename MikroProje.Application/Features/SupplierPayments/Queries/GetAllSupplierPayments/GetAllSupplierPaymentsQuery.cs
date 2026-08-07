using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.SupplierPayments.DTOs;

namespace MikroProje.Application.Features.SupplierPayments.Queries.GetAllSupplierPayments;

public class GetAllSupplierPaymentsQuery : IRequest<Result<PagedResult<SupplierPaymentListDto>>>
{
    public int? CurrentAccountId { get; set; }
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
