using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.AuditLogs.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.AuditLogs.Queries.ExportAuditLogsExcel;

public class ExportAuditLogsExcelQueryHandler : IRequestHandler<ExportAuditLogsExcelQuery, Result<ExcelExportResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IExcelExportService _excelExportService;
    private readonly IConfiguration _configuration;

    public ExportAuditLogsExcelQueryHandler(IApplicationDbContext dbContext, IExcelExportService excelExportService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _excelExportService = excelExportService;
        _configuration = configuration;
    }

    public async Task<Result<ExcelExportResult>> Handle(ExportAuditLogsExcelQuery request, CancellationToken cancellationToken)
    {
        int maxRowCount = 20000;
        if (int.TryParse(_configuration["ExcelExport:MaxRowCount"], out var configuredMax))
        {
            maxRowCount = configuredMax;
        }

        var query = _dbContext.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(x => x.UserId == request.UserId);
        }

        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            query = query.Where(x => x.Username != null && x.Username.Contains(request.Username));
        }

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            query = query.Where(x => x.EntityName == request.EntityName);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityId))
        {
            query = query.Where(x => x.EntityId == request.EntityId);
        }

        if (request.Action.HasValue)
        {
            query = query.Where(x => x.Action == request.Action.Value);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreatedDate >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreatedDate <= request.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var searchTerm = request.Search.ToLower();
            query = query.Where(x => 
                (x.Username != null && x.Username.ToLower().Contains(searchTerm)) ||
                x.EntityName.ToLower().Contains(searchTerm) ||
                (x.RequestPath != null && x.RequestPath.ToLower().Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount > maxRowCount)
        {
            return Result<ExcelExportResult>.Fail($"Excel aktarýmý en fazla {maxRowCount:N0} kayýt desteklemektedir. Lütfen filtreleri daraltýn.", 400);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new AuditLogExportDto
            {
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                UserId = x.UserId,
                Username = x.Username,
                RequestPath = x.RequestPath,
                IpAddress = x.IpAddress,
                CorrelationId = x.CorrelationId,
                CreatedDate = x.CreatedDate,
                ChangedColumns = x.ChangedColumns
            })
            .ToListAsync(cancellationToken);

        var result = await _excelExportService.ExportAsync(items, "Ýþlem Geçmiþi", cancellationToken);
        return Result<ExcelExportResult>.Ok(result);
    }
}



