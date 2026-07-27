using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Sales.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Sales.Queries.ExportSalesPdf;

public class ExportSalesPdfQueryHandler : IRequestHandler<ExportSalesPdfQuery, Result<PdfExportResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPdfExportService _pdfExportService;
    private readonly IConfiguration _configuration;

    public ExportSalesPdfQueryHandler(IApplicationDbContext dbContext, IPdfExportService pdfExportService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _pdfExportService = pdfExportService;
        _configuration = configuration;
    }

    public async Task<Result<PdfExportResult>> Handle(ExportSalesPdfQuery request, CancellationToken cancellationToken)
    {
        int maxRowCount = 5000;
        if (int.TryParse(_configuration["PdfExport:MaxRowCount"], out var configuredMax))
        {
            maxRowCount = configuredMax;
        }

        var query = _dbContext.Sales
            .Include(x => x.CurrentAccount)
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount > maxRowCount)
        {
            return Result<PdfExportResult>.Fail($"PDF aktarýmý en fazla {maxRowCount:N0} kayýt desteklemektedir. Lütfen filtreleri daraltýn.", 400);
        }

        var items = await query
            .OrderByDescending(x => x.SaleDate)
            .Select(x => new SalePdfExportDto
            {
                SaleNumber = x.Id.ToString(),
                SaleDate = x.SaleDate,
                CurrentAccountCode = x.CurrentAccount.Code,
                CurrentAccountName = x.CurrentAccount.Name,
                TotalAmount = x.TotalAmount,
                VatAmount = x.VatAmount,
                GrandTotal = x.GrandTotal,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);

        var result = await _pdfExportService.ExportAsync(items, "Satýþ Listesi", "Sales", cancellationToken);
        return Result<PdfExportResult>.Ok(result);
    }
}
