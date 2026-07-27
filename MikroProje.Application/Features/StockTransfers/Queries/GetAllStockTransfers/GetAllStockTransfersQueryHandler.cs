using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;
using MikroProje.Application.Features.StockTransfers.DTOs;

namespace MikroProje.Application.Features.StockTransfers.Queries.GetAllStockTransfers;

public class GetAllStockTransfersQueryHandler : IRequestHandler<GetAllStockTransfersQuery, Result<PagedResult<StockTransferListDto>>>
{
    private readonly IStockTransferRepository _repository;

    public GetAllStockTransfersQueryHandler(IStockTransferRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<StockTransferListDto>>> Handle(GetAllStockTransfersQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetAllPagedAsync(
            request.Search,
            request.SourceWarehouseId,
            request.DestinationWarehouseId,
            request.Status,
            request.StartDate,
            request.EndDate,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result<PagedResult<StockTransferListDto>>.Ok(result);
    }
}
