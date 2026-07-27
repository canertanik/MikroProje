using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.CurrentAccounts.Queries.ExportCurrentAccountsPdf;

public class ExportCurrentAccountsPdfQueryHandler : IRequestHandler<ExportCurrentAccountsPdfQuery, Result<PdfExportResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPdfExportService _pdfExportService;
    private readonly IConfiguration _configuration;

    public ExportCurrentAccountsPdfQueryHandler(IApplicationDbContext dbContext, IPdfExportService pdfExportService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _pdfExportService = pdfExportService;
        _configuration = configuration;
    }

    public async Task<Result<PdfExportResult>> Handle(ExportCurrentAccountsPdfQuery request, CancellationToken cancellationToken)
    {
        int maxRowCount = 5000;
        if (int.TryParse(_configuration["PdfExport:MaxRowCount"], out var configuredMax))
        {
            maxRowCount = configuredMax;
        }

        var query = _dbContext.CurrentAccounts
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount > maxRowCount)
        {
            return Result<PdfExportResult>.Fail($"PDF aktarýmý en fazla {maxRowCount:N0} kayýt desteklemektedir. Lütfen filtreleri daraltýn.", 400);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new CurrentAccountPdfExportDto
            {
                Code = x.Code,
                Name = x.Name,
                AccountType = x.Type.ToString(),
                TaxNumber = x.TaxNumber,
                Phone = x.Phone,
                Email = x.Email,
                Balance = x.Balance
            })
            .ToListAsync(cancellationToken);

        var result = await _pdfExportService.ExportAsync(items, "Cari Hesaplar", "CurrentAccounts", cancellationToken);
        return Result<PdfExportResult>.Ok(result);
    }
}
