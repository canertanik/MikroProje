using MikroProje.Domain.Common;

namespace MikroProje.Domain.Entities;

public class Product : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal SalePrice { get; set; }

    public decimal VatRate { get; set; }

    public int StockQuantity { get; set; }

    public int CriticalStockQuantity { get; set; }

    public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
