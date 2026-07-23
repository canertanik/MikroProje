using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Sales.DTOs;

namespace MikroProje.Application.Features.Sales.Queries.GetSalesByCurrentAccount;

public class GetSalesByCurrentAccountQuery : IRequest<Result<PagedResult<SaleDto>>>
{
    public int CurrentAccountId { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
