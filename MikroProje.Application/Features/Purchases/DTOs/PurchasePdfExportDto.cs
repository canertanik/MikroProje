using MikroProje.Application.Common.Pdf;

namespace MikroProje.Application.Features.Purchases.DTOs;

public class PurchasePdfExportDto
{
    [PdfColumn(Header = "Alýþ No", Order = 1, Width = 1)]
    public string PurchaseNumber { get; set; } = string.Empty;

    [PdfColumn(Header = "Tarih", Order = 2, Width = 1, Format = "dd.MM.yyyy HH:mm")]
    public DateTime PurchaseDate { get; set; }

    [PdfColumn(Header = "Tedarikçi Kodu", Order = 3, Width = 1)]
    public string SupplierCode { get; set; } = string.Empty;

    [PdfColumn(Header = "Tedarikçi Adý", Order = 4, Width = 2)]
    public string SupplierName { get; set; } = string.Empty;

    [PdfColumn(Header = "Ara Toplam", Order = 5, Width = 1, Alignment = "Right", Format = "#,##0.00")]
    public decimal Subtotal { get; set; }

    [PdfColumn(Header = "KDV Toplamý", Order = 6, Width = 1, Alignment = "Right", Format = "#,##0.00")]
    public decimal VatAmount { get; set; }

    [PdfColumn(Header = "Genel Toplam", Order = 7, Width = 1, Alignment = "Right", Format = "#,##0.00")]
    public decimal GrandTotal { get; set; }

    [PdfColumn(Header = "Açýklama", Order = 8, Width = 2)]
    public string? Description { get; set; }
}
