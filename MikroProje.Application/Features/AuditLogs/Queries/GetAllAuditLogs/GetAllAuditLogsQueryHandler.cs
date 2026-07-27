using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.AuditLogs.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.AuditLogs.Queries.GetAllAuditLogs;

public class GetAllAuditLogsQueryHandler : IRequestHandler<GetAllAuditLogsQuery, Result<PagedResult<AuditLogListDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetAllAuditLogsQueryHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<AuditLogListDto>>> Handle(GetAllAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.UserId))
            query = query.Where(x => x.UserId == request.UserId);

        if (!string.IsNullOrWhiteSpace(request.Username))
            query = query.Where(x => x.Username != null && x.Username.Contains(request.Username));

        if (!string.IsNullOrWhiteSpace(request.EntityName))
            query = query.Where(x => x.EntityName == request.EntityName);

        if (!string.IsNullOrWhiteSpace(request.EntityId))
            query = query.Where(x => x.EntityId == request.EntityId);

        if (request.Action.HasValue)
            query = query.Where(x => x.Action == request.Action.Value);

        if (request.StartDate.HasValue)
        {
            var utcStartDate = request.StartDate.Value.ToUniversalTime();
            query = query.Where(x => x.CreatedDate >= utcStartDate);
        }

        if (request.EndDate.HasValue)
        {
            var utcEndDate = request.EndDate.Value.ToUniversalTime();
            query = query.Where(x => x.CreatedDate <= utcEndDate);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(x =>
                (x.Username != null && x.Username.ToLower().Contains(search)) ||
                x.EntityName.ToLower().Contains(search) ||
                (x.EntityId != null && x.EntityId.ToLower().Contains(search)) ||
                (x.ChangedColumns != null && x.ChangedColumns.ToLower().Contains(search)));
        }

        query = query.OrderByDescending(x => x.CreatedDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<AuditLogListDto>>(items);
        
        var pagedResult = PagedResult<AuditLogListDto>.Create(dtos, request.PageNumber, request.PageSize, totalCount);

        return Result<PagedResult<AuditLogListDto>>.Ok(pagedResult);
    }
}
