using MediatR;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.StockTransfers.Commands.CompleteStockTransfer;

public class CompleteStockTransferCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
