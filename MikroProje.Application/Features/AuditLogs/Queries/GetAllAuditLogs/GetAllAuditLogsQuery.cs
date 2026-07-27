using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.AuditLogs.DTOs;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.AuditLogs.Queries.GetAllAuditLogs;

public class GetAllAuditLogsQuery : IRequest<Result<PagedResult<AuditLogListDto>>>
{
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public AuditAction? Action { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
