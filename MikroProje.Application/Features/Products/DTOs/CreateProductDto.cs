namespace MikroProje.Application.Features.Products.DTOs;

public class CreateProductDto
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SalePrice { get; set; }

    public decimal VatRate { get; set; }

    public int CriticalStockQuantity { get; set; }

    public int InitialStockQuantity { get; set; }
}
