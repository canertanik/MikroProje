namespace MikroProje.Application.Features.StockTransfers.DTOs;

public class CreateStockTransferItemRequestDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
