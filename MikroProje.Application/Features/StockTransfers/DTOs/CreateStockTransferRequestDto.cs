namespace MikroProje.Application.Features.StockTransfers.DTOs;

public class CreateStockTransferRequestDto
{
    public int SourceWarehouseId { get; set; }
    public int DestinationWarehouseId { get; set; }
    public DateTime? TransferDate { get; set; }
    public string? Description { get; set; }
    public List<CreateStockTransferItemRequestDto> Items { get; set; } = new();
}
