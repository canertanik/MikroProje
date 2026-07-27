using MikroProje.Application.Common.Pdf;

namespace MikroProje.Application.Features.Products.DTOs;

public class ProductPdfExportDto
{
    [PdfColumn(Header = "Ürün Kodu", Order = 1, Width = 1)]
    public string Code { get; set; } = string.Empty;

    [PdfColumn(Header = "Ürün Adý", Order = 2, Width = 2)]
    public string Name { get; set; } = string.Empty;

    [PdfColumn(Header = "Barkod", Order = 3, Width = 1)]
    public string? Barcode { get; set; }

    [PdfColumn(Header = "Alýþ Fiyatý", Order = 4, Width = 1, Alignment = "Right", Format = "#,##0.00")]
    public decimal PurchasePrice { get; set; }

    [PdfColumn(Header = "Satýþ Fiyatý", Order = 5, Width = 1, Alignment = "Right", Format = "#,##0.00")]
    public decimal SalePrice { get; set; }

    [PdfColumn(Header = "KDV", Order = 6, Width = 1, Alignment = "Right", Format = "0.00")]
    public decimal VatRate { get; set; }

    [PdfColumn(Header = "Stok", Order = 7, Width = 1, Alignment = "Right", Format = "#,##0")]
    public int StockQuantity { get; set; }
}
