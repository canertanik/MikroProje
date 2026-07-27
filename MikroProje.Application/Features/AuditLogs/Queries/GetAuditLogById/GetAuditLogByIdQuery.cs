using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.AuditLogs.DTOs;

namespace MikroProje.Application.Features.AuditLogs.Queries.GetAuditLogById;

public class GetAuditLogByIdQuery : IRequest<Result<AuditLogDto>>
{
    public int Id { get; set; }
}
