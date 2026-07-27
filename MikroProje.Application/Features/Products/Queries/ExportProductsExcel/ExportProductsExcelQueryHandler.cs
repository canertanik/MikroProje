using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Products.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Products.Queries.ExportProductsExcel;

public class ExportProductsExcelQueryHandler : IRequestHandler<ExportProductsExcelQuery, Result<ExcelExportResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IExcelExportService _excelExportService;
    private readonly IConfiguration _configuration;

    public ExportProductsExcelQueryHandler(IApplicationDbContext dbContext, IExcelExportService excelExportService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _excelExportService = excelExportService;
        _configuration = configuration;
    }

    public async Task<Result<ExcelExportResult>> Handle(ExportProductsExcelQuery request, CancellationToken cancellationToken)
    {
        int maxRowCount = 20000;
        if (int.TryParse(_configuration["ExcelExport:MaxRowCount"], out var configuredMax))
        {
            maxRowCount = configuredMax;
        }

        var query = _dbContext.Products
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(x => x.Code.Contains(request.Search) || x.Name.Contains(request.Search) || (x.Barcode != null && x.Barcode.Contains(request.Search)));
        }

        if (request.CriticalOnly == true)
        {
            query = query.Where(x => x.StockQuantity <= x.CriticalStockQuantity);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount > maxRowCount)
        {
            return Result<ExcelExportResult>.Fail($"Excel aktarýmý en fazla {maxRowCount:N0} kayýt desteklemektedir. Lütfen filtreleri daraltýn.", 400);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new ProductExportDto
            {
                Code = x.Code,
                Name = x.Name,
                Barcode = x.Barcode,
                PurchasePrice = x.PurchasePrice,
                SalePrice = x.SalePrice,
                VatRate = x.VatRate,
                StockQuantity = x.StockQuantity,
                CriticalStockQuantity = x.CriticalStockQuantity
            })
            .ToListAsync(cancellationToken);

        var result = await _excelExportService.ExportAsync(items, "Ürünler", cancellationToken);
        return Result<ExcelExportResult>.Ok(result);
    }
}




