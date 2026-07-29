namespace MikroProje.Application.Features.Purchases.DTOs;

public class PurchaseDto
{
    public int Id { get; set; }
    public int CurrentAccountId { get; set; }
    public string CurrentAccountName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal VatAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Description { get; set; }
    public List<PurchaseItemDto> Items { get; set; } = new();
}
