using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Purchases.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Purchases.Queries.ExportPurchasesPdf;

public class ExportPurchasesPdfQueryHandler : IRequestHandler<ExportPurchasesPdfQuery, Result<PdfExportResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPdfExportService _pdfExportService;
    private readonly IConfiguration _configuration;

    public ExportPurchasesPdfQueryHandler(IApplicationDbContext dbContext, IPdfExportService pdfExportService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _pdfExportService = pdfExportService;
        _configuration = configuration;
    }

    public async Task<Result<PdfExportResult>> Handle(ExportPurchasesPdfQuery request, CancellationToken cancellationToken)
    {
        int maxRowCount = 5000;
        if (int.TryParse(_configuration["PdfExport:MaxRowCount"], out var configuredMax))
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
            return Result<PdfExportResult>.Fail($"PDF aktarýmý en fazla {maxRowCount:N0} kayýt desteklemektedir. Lütfen filtreleri daraltýn.", 400);
        }

        var items = await query
            .OrderByDescending(x => x.PurchaseDate)
            .Select(x => new PurchasePdfExportDto
            {
                PurchaseNumber = x.Id.ToString(),
                PurchaseDate = x.PurchaseDate,
                SupplierCode = x.CurrentAccount.Code,
                SupplierName = x.CurrentAccount.Name,
                Subtotal = x.Subtotal,
                VatAmount = x.VatAmount,
                GrandTotal = x.GrandTotal,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);

        var result = await _pdfExportService.ExportAsync(items, "Alýþ Listesi", "Purchases", cancellationToken);
        return Result<PdfExportResult>.Ok(result);
    }
}
