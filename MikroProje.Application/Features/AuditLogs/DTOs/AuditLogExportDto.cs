using MikroProje.Application.Common.Excel;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.AuditLogs.DTOs;

public class AuditLogExportDto
{
    [ExcelColumn(Header = "Ýþlem", Order = 1)]
    public AuditAction Action { get; set; }

    [ExcelColumn(Header = "Kayýt Tipi", Order = 2)]
    public string EntityName { get; set; } = string.Empty;

    [ExcelColumn(Header = "Kayýt ID", Order = 3)]
    public string? EntityId { get; set; }

    [ExcelColumn(Header = "Kullanýcý ID", Order = 4)]
    public string? UserId { get; set; }

    [ExcelColumn(Header = "Kullanýcý Adý", Order = 5)]
    public string? Username { get; set; }

    [ExcelColumn(Header = "Ýstek Yolu", Order = 6)]
    public string? RequestPath { get; set; }

    [ExcelColumn(Header = "IP Adresi", Order = 7)]
    public string? IpAddress { get; set; }

    [ExcelColumn(Header = "Ýzleme (Correlation) ID", Order = 8)]
    public string? CorrelationId { get; set; }

    [ExcelColumn(Header = "Oluþturulma Tarihi", Order = 9, NumberFormat = "dd.MM.yyyy HH:mm")]
    public DateTime CreatedDate { get; set; }

    [ExcelColumn(Header = "Deðiþen Kolonlar", Order = 10)]
    public string? ChangedColumns { get; set; }
}
