namespace MikroProje.Application.Features.Products.DTOs;

public class ProductStockDto
{
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
