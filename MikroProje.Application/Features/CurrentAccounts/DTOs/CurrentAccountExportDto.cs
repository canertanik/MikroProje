using MikroProje.Application.Common.Excel;

namespace MikroProje.Application.Features.CurrentAccounts.DTOs;

public class CurrentAccountExportDto
{
    [ExcelColumn(Header = "Cari Kodu", Order = 1)]
    public string Code { get; set; } = string.Empty;

    [ExcelColumn(Header = "Cari Adý", Order = 2)]
    public string Name { get; set; } = string.Empty;

    [ExcelColumn(Header = "Hesap Tipi", Order = 3)]
    public string AccountType { get; set; } = string.Empty;

    [ExcelColumn(Header = "Vergi Numarasý", Order = 4)]
    public string? TaxNumber { get; set; }

    [ExcelColumn(Header = "Telefon", Order = 5)]
    public string? Phone { get; set; }

    [ExcelColumn(Header = "E-Posta", Order = 6)]
    public string? Email { get; set; }

    [ExcelColumn(Header = "Bakiye", Order = 7, NumberFormat = "#,##0.00")]
    public decimal Balance { get; set; }
}
