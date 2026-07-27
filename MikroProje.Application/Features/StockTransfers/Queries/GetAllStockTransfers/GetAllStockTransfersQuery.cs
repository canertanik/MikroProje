using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.StockTransfers.DTOs;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.StockTransfers.Queries.GetAllStockTransfers;

public class GetAllStockTransfersQuery : IRequest<Result<PagedResult<StockTransferListDto>>>
{
    public string? Search { get; set; }
    public int? SourceWarehouseId { get; set; }
    public int? DestinationWarehouseId { get; set; }
    public StockTransferStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
