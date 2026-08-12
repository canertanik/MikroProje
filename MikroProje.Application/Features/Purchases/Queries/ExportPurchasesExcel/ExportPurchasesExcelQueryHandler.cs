using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Purchases.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Purchases.Queries.ExportPurchasesExcel;

public class ExportPurchasesExcelQueryHandler : IRequestHandler<ExportPurchasesExcelQuery, Result<ExcelExportResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IExcelExportService _excelExportService;
    private readonly IConfiguration _configuration;

    public ExportPurchasesExcelQueryHandler(IApplicationDbContext dbContext, IExcelExportService excelExportService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _excelExportService = excelExportService;
        _configuration = configuration;
    }

    public async Task<Result<ExcelExportResult>> Handle(ExportPurchasesExcelQuery request, CancellationToken cancellationToken)
    {
        int maxRowCount = 20000;
        if (int.TryParse(_configuration["ExcelExport:MaxRowCount"], out var configuredMax))
        {
            maxRowCount = configuredMax;
        }

        var query = _dbContext.Purchases
            .Include(x => x.CurrentAccount)
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount > maxRowCount)
        {
            return Result<ExcelExportResult>.Fail($"Excel aktar�m� en fazla {maxRowCount:N0} kay�t desteklemektedir. L�tfen filtreleri daralt�n.", 400);
        }

        var items = await query
            .OrderByDescending(x => x.PurchaseDate)
            .Select(x => new PurchaseExportDto
            {
                PurchaseNumber = x.Id.ToString(),
                PurchaseDate = x.PurchaseDate,
                SupplierCode = x.CurrentAccount != null ? x.CurrentAccount.Code : string.Empty,
                SupplierName = x.CurrentAccount != null ? x.CurrentAccount.Name : string.Empty,
                Subtotal = x.Subtotal,
                VatAmount = x.VatAmount,
                GrandTotal = x.GrandTotal,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);

        var result = await _excelExportService.ExportAsync(items, "Al��lar", cancellationToken);
        return Result<ExcelExportResult>.Ok(result);
    }
}




