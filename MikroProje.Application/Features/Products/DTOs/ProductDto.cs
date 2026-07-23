namespace MikroProje.Application.Features.Products.DTOs;

public class ProductDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SalePrice { get; set; }

    public decimal VatRate { get; set; }

    public int StockQuantity { get; set; }

    public int CriticalStockQuantity { get; set; }

    public bool IsCriticalStock { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
