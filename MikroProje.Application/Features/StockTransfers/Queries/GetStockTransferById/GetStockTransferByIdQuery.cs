using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.StockTransfers.DTOs;

namespace MikroProje.Application.Features.StockTransfers.Queries.GetStockTransferById;

public class GetStockTransferByIdQuery : IRequest<Result<StockTransferDto>>
{
    public int Id { get; set; }
}
