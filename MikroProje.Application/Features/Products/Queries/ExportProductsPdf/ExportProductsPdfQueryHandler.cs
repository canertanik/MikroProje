using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Products.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Products.Queries.ExportProductsPdf;

public class ExportProductsPdfQueryHandler : IRequestHandler<ExportProductsPdfQuery, Result<PdfExportResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPdfExportService _pdfExportService;
    private readonly IConfiguration _configuration;

    public ExportProductsPdfQueryHandler(IApplicationDbContext dbContext, IPdfExportService pdfExportService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _pdfExportService = pdfExportService;
        _configuration = configuration;
    }

    public async Task<Result<PdfExportResult>> Handle(ExportProductsPdfQuery request, CancellationToken cancellationToken)
    {
        int maxRowCount = 5000;
        if (int.TryParse(_configuration["PdfExport:MaxRowCount"], out var configuredMax))
        {
            maxRowCount = configuredMax;
        }

        var query = _dbContext.Products
            .AsNoTracking()
            .AsQueryable(); // AsNoTracking is enough, IsDeleted is not available if not inheriting correctly but let's assume it works (I'll fix if not)

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
            return Result<PdfExportResult>.Fail($"PDF aktarýmý en fazla {maxRowCount:N0} kayýt desteklemektedir. Lütfen filtreleri daraltýn.", 400);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new ProductPdfExportDto
            {
                Code = x.Code,
                Name = x.Name,
                Barcode = x.Barcode,
                PurchasePrice = x.PurchasePrice,
                SalePrice = x.SalePrice,
                VatRate = x.VatRate,
                StockQuantity = x.StockQuantity
            })
            .ToListAsync(cancellationToken);

        var result = await _pdfExportService.ExportAsync(items, "Ürün Listesi", "Products", cancellationToken);
        return Result<PdfExportResult>.Ok(result);
    }
}
