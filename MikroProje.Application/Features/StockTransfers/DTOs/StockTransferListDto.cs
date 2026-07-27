using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.StockTransfers.DTOs;

public class StockTransferListDto
{
    public int Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public string SourceWarehouseCode { get; set; } = string.Empty;
    public string DestinationWarehouseCode { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public StockTransferStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
}
