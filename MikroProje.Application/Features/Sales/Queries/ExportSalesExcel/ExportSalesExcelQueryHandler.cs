using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Sales.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Sales.Queries.ExportSalesExcel;

public class ExportSalesExcelQueryHandler : IRequestHandler<ExportSalesExcelQuery, Result<ExcelExportResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IExcelExportService _excelExportService;
    private readonly IConfiguration _configuration;

    public ExportSalesExcelQueryHandler(IApplicationDbContext dbContext, IExcelExportService excelExportService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _excelExportService = excelExportService;
        _configuration = configuration;
    }

    public async Task<Result<ExcelExportResult>> Handle(ExportSalesExcelQuery request, CancellationToken cancellationToken)
    {
        int maxRowCount = 20000;
        if (int.TryParse(_configuration["ExcelExport:MaxRowCount"], out var configuredMax))
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
            return Result<ExcelExportResult>.Fail($"Excel aktarýmý en fazla {maxRowCount:N0} kayýt desteklemektedir. Lütfen filtreleri daraltýn.", 400);
        }

        var items = await query
            .OrderByDescending(x => x.SaleDate)
            .Select(x => new SaleExportDto
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

        var result = await _excelExportService.ExportAsync(items, "Satýþlar", cancellationToken);
        return Result<ExcelExportResult>.Ok(result);
    }
}




