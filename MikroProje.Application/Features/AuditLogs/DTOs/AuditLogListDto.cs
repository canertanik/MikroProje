using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.AuditLogs.DTOs;

public class AuditLogListDto
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public DateTime CreatedDate { get; set; }
}
