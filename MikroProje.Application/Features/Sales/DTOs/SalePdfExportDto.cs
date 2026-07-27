using MikroProje.Application.Common.Pdf;

namespace MikroProje.Application.Features.Sales.DTOs;

public class SalePdfExportDto
{
    [PdfColumn(Header = "Satýþ No", Order = 1, Width = 1)]
    public string SaleNumber { get; set; } = string.Empty;

    [PdfColumn(Header = "Tarih", Order = 2, Width = 1, Format = "dd.MM.yyyy HH:mm")]
    public DateTime SaleDate { get; set; }

    [PdfColumn(Header = "Cari Kodu", Order = 3, Width = 1)]
    public string CurrentAccountCode { get; set; } = string.Empty;

    [PdfColumn(Header = "Cari Adý", Order = 4, Width = 2)]
    public string CurrentAccountName { get; set; } = string.Empty;

    [PdfColumn(Header = "Ara Toplam", Order = 5, Width = 1, Alignment = "Right", Format = "#,##0.00")]
    public decimal TotalAmount { get; set; }

    [PdfColumn(Header = "KDV Toplamý", Order = 6, Width = 1, Alignment = "Right", Format = "#,##0.00")]
    public decimal VatAmount { get; set; }

    [PdfColumn(Header = "Genel Toplam", Order = 7, Width = 1, Alignment = "Right", Format = "#,##0.00")]
    public decimal GrandTotal { get; set; }

    [PdfColumn(Header = "Açýklama", Order = 8, Width = 2)]
    public string? Description { get; set; }
}
