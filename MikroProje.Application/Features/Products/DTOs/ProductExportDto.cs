using MikroProje.Application.Common.Excel;

namespace MikroProje.Application.Features.Products.DTOs;

public class ProductExportDto
{
    [ExcelColumn(Header = "Ürün Kodu", Order = 1)]
    public string Code { get; set; } = string.Empty;

    [ExcelColumn(Header = "Ürün Adý", Order = 2)]
    public string Name { get; set; } = string.Empty;

    [ExcelColumn(Header = "Barkod", Order = 3)]
    public string? Barcode { get; set; }

    [ExcelColumn(Header = "Alýþ Fiyatý", Order = 4, NumberFormat = "#,##0.00")]
    public decimal PurchasePrice { get; set; }

    [ExcelColumn(Header = "Satýþ Fiyatý", Order = 5, NumberFormat = "#,##0.00")]
    public decimal SalePrice { get; set; }

    [ExcelColumn(Header = "KDV Oraný", Order = 6, NumberFormat = "0.00")]
    public decimal VatRate { get; set; }

    [ExcelColumn(Header = "Stok Miktarý", Order = 7, NumberFormat = "#,##0")]
    public int StockQuantity { get; set; }

    [ExcelColumn(Header = "Kritik Stok", Order = 8, NumberFormat = "#,##0")]
    public int CriticalStockQuantity { get; set; }
}
