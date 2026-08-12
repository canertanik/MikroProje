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
            return Result<PdfExportResult>.Fail($"PDF aktar�m� en fazla {maxRowCount:N0} kay�t desteklemektedir. L�tfen filtreleri daralt�n.", 400);
        }

        var items = await query
            .OrderByDescending(x => x.PurchaseDate)
            .Select(x => new PurchasePdfExportDto
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

        var result = await _pdfExportService.ExportAsync(items, "Al�� Listesi", "Purchases", cancellationToken);
        return Result<PdfExportResult>.Ok(result);
    }
}
