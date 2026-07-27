using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.AuditLogs.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.AuditLogs.Queries.GetAuditLogById;

public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, Result<AuditLogDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetAuditLogByIdQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Result<AuditLogDto>> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
    {
        var auditLog = await _dbContext.AuditLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (auditLog == null)
            return Result<AuditLogDto>.Fail("Audit kaydý bulunamadý.", 404);

        var dto = _mapper.Map<AuditLogDto>(auditLog);
        return Result<AuditLogDto>.Ok(dto);
    }
}
