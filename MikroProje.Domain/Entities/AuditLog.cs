using MikroProje.Domain.Enums;
using MikroProje.Domain.Common;

namespace MikroProje.Domain.Entities;

public class AuditLog : IAuditIgnore
{
    public int Id { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public AuditAction Action { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedColumns { get; set; }
    public string? IpAddress { get; set; }
    public string? RequestPath { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
}
