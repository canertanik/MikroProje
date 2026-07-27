using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.StockTransfers.DTOs;

public class StockTransferDto
{
    public int Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public int SourceWarehouseId { get; set; }
    public string SourceWarehouseCode { get; set; } = string.Empty;
    public string SourceWarehouseName { get; set; } = string.Empty;
    public int DestinationWarehouseId { get; set; }
    public string DestinationWarehouseCode { get; set; } = string.Empty;
    public string DestinationWarehouseName { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public string? Description { get; set; }
    public StockTransferStatus Status { get; set; }
    public List<StockTransferItemDto> Items { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
