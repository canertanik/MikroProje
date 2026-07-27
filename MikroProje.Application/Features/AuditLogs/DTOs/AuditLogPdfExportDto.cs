using MikroProje.Application.Common.Pdf;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.AuditLogs.DTOs;

public class AuditLogPdfExportDto
{
    [PdfColumn(Header = "Tarih", Order = 1, Width = 1, Format = "dd.MM.yyyy HH:mm")]
    public DateTime CreatedDate { get; set; }

    [PdfColumn(Header = "Ýþlem", Order = 2, Width = 1)]
    public AuditAction Action { get; set; }

    [PdfColumn(Header = "Kayýt Tipi", Order = 3, Width = 1)]
    public string EntityName { get; set; } = string.Empty;

    [PdfColumn(Header = "Kayýt ID", Order = 4, Width = 1)]
    public string? EntityId { get; set; }

    [PdfColumn(Header = "Kullanýcý", Order = 5, Width = 1)]
    public string? Username { get; set; }

    [PdfColumn(Header = "IP Adresi", Order = 6, Width = 1)]
    public string? IpAddress { get; set; }

    [PdfColumn(Header = "Deðiþen Kolonlar", Order = 7, Width = 2)]
    public string? ChangedColumns { get; set; }
}
