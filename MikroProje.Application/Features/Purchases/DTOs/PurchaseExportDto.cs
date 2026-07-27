using MikroProje.Application.Common.Excel;

namespace MikroProje.Application.Features.Purchases.DTOs;

public class PurchaseExportDto
{
    [ExcelColumn(Header = "Alýþ Numarasý", Order = 1)]
    public string PurchaseNumber { get; set; } = string.Empty;

    [ExcelColumn(Header = "Tarih", Order = 2, NumberFormat = "dd.MM.yyyy HH:mm")]
    public DateTime PurchaseDate { get; set; }

    [ExcelColumn(Header = "Tedarikçi Kodu", Order = 3)]
    public string SupplierCode { get; set; } = string.Empty;

    [ExcelColumn(Header = "Tedarikçi Adý", Order = 4)]
    public string SupplierName { get; set; } = string.Empty;

    [ExcelColumn(Header = "Ara Toplam", Order = 5, NumberFormat = "#,##0.00")]
    public decimal Subtotal { get; set; }

    [ExcelColumn(Header = "KDV Toplamý", Order = 6, NumberFormat = "#,##0.00")]
    public decimal VatAmount { get; set; }

    [ExcelColumn(Header = "Genel Toplam", Order = 7, NumberFormat = "#,##0.00")]
    public decimal GrandTotal { get; set; }

    [ExcelColumn(Header = "Açýklama", Order = 8)]
    public string? Description { get; set; }
}
