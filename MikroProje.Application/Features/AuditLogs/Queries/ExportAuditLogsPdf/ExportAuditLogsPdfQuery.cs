using MediatR;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Common.Results;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.AuditLogs.Queries.ExportAuditLogsPdf;

public class ExportAuditLogsPdfQuery : IRequest<Result<PdfExportResult>>
{
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public AuditAction? Action { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Search { get; set; }
}
