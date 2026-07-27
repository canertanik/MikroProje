using MikroProje.Application.Common.Pdf;

namespace MikroProje.Application.Features.CurrentAccounts.DTOs;

public class CurrentAccountPdfExportDto
{
    [PdfColumn(Header = "Cari Kodu", Order = 1, Width = 1)]
    public string Code { get; set; } = string.Empty;

    [PdfColumn(Header = "Cari Adý", Order = 2, Width = 2)]
    public string Name { get; set; } = string.Empty;

    [PdfColumn(Header = "Hesap Tipi", Order = 3, Width = 1)]
    public string AccountType { get; set; } = string.Empty;

    [PdfColumn(Header = "Vergi Numarasý", Order = 4, Width = 1)]
    public string? TaxNumber { get; set; }

    [PdfColumn(Header = "Telefon", Order = 5, Width = 1)]
    public string? Phone { get; set; }

    [PdfColumn(Header = "E-Posta", Order = 6, Width = 1)]
    public string? Email { get; set; }

    [PdfColumn(Header = "Bakiye", Order = 7, Width = 1, Alignment = "Right", Format = "#,##0.00")]
    public decimal Balance { get; set; }
}
