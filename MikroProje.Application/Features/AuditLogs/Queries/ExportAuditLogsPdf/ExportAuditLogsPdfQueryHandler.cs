using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.AuditLogs.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.AuditLogs.Queries.ExportAuditLogsPdf;

public class ExportAuditLogsPdfQueryHandler : IRequestHandler<ExportAuditLogsPdfQuery, Result<PdfExportResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPdfExportService _pdfExportService;
    private readonly IConfiguration _configuration;

    public ExportAuditLogsPdfQueryHandler(IApplicationDbContext dbContext, IPdfExportService pdfExportService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _pdfExportService = pdfExportService;
        _configuration = configuration;
    }

    public async Task<Result<PdfExportResult>> Handle(ExportAuditLogsPdfQuery request, CancellationToken cancellationToken)
    {
        int maxRowCount = 5000;
        if (int.TryParse(_configuration["PdfExport:MaxRowCount"], out var configuredMax))
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
            return Result<PdfExportResult>.Fail($"PDF aktarýmý en fazla {maxRowCount:N0} kayýt desteklemektedir. Lütfen filtreleri daraltýn.", 400);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new AuditLogPdfExportDto
            {
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Username = x.Username,
                IpAddress = x.IpAddress,
                CreatedDate = x.CreatedDate,
                ChangedColumns = x.ChangedColumns
            })
            .ToListAsync(cancellationToken);

        var result = await _pdfExportService.ExportAsync(items, "Ýþlem Geçmiþi (Audit Logs)", "AuditLogs", cancellationToken);
        return Result<PdfExportResult>.Ok(result);
    }
}
