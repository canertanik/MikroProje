using MikroProje.Application.Common.Excel;

namespace MikroProje.Application.Features.Sales.DTOs;

public class SaleExportDto
{
    [ExcelColumn(Header = "Satýþ Numarasý", Order = 1)]
    public string SaleNumber { get; set; } = string.Empty;

    [ExcelColumn(Header = "Tarih", Order = 2, NumberFormat = "dd.MM.yyyy HH:mm")]
    public DateTime SaleDate { get; set; }

    [ExcelColumn(Header = "Cari Kodu", Order = 3)]
    public string CurrentAccountCode { get; set; } = string.Empty;

    [ExcelColumn(Header = "Cari Adý", Order = 4)]
    public string CurrentAccountName { get; set; } = string.Empty;

    [ExcelColumn(Header = "Ara Toplam", Order = 5, NumberFormat = "#,##0.00")]
    public decimal TotalAmount { get; set; }

    [ExcelColumn(Header = "KDV Toplamý", Order = 6, NumberFormat = "#,##0.00")]
    public decimal VatAmount { get; set; }

    [ExcelColumn(Header = "Genel Toplam", Order = 7, NumberFormat = "#,##0.00")]
    public decimal GrandTotal { get; set; }

    [ExcelColumn(Header = "Açýklama", Order = 8)]
    public string? Description { get; set; }
}
