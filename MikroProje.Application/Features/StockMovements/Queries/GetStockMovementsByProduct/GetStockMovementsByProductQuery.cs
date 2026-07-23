using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.StockMovements.DTOs;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.StockMovements.Queries.GetStockMovementsByProduct;

public class GetStockMovementsByProductQuery : IRequest<Result<PagedResult<StockMovementDto>>>
{
    public int ProductId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public StockMovementType? MovementType { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
