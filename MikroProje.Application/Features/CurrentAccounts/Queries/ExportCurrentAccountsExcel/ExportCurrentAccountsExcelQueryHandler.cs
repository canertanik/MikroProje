using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.CurrentAccounts.Queries.ExportCurrentAccountsExcel;

public class ExportCurrentAccountsExcelQueryHandler : IRequestHandler<ExportCurrentAccountsExcelQuery, Result<ExcelExportResult>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IExcelExportService _excelExportService;
    private readonly IConfiguration _configuration;

    public ExportCurrentAccountsExcelQueryHandler(IApplicationDbContext dbContext, IExcelExportService excelExportService, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _excelExportService = excelExportService;
        _configuration = configuration;
    }

    public async Task<Result<ExcelExportResult>> Handle(ExportCurrentAccountsExcelQuery request, CancellationToken cancellationToken)
    {
        int maxRowCount = 20000;
        if (int.TryParse(_configuration["ExcelExport:MaxRowCount"], out var configuredMax))
        {
            maxRowCount = configuredMax;
        }

        var query = _dbContext.CurrentAccounts
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);
        if (totalCount > maxRowCount)
        {
            return Result<ExcelExportResult>.Fail($"Excel aktarýmý en fazla {maxRowCount:N0} kayýt desteklemektedir. Lütfen filtreleri daraltýn.", 400);
        }

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new CurrentAccountExportDto
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

        var result = await _excelExportService.ExportAsync(items, "Cariler", cancellationToken);
        return Result<ExcelExportResult>.Ok(result);
    }
}




