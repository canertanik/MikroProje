using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.StockTransfers.DTOs;

namespace MikroProje.Application.Features.StockTransfers.Commands.CreateStockTransfer;

public class CreateStockTransferCommand : IRequest<Result<StockTransferDto>>
{
    public CreateStockTransferRequestDto Dto { get; set; } = null!;
}
