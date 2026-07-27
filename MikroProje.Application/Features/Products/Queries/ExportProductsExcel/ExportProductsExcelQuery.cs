using MediatR;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.Products.Queries.ExportProductsExcel;

public class ExportProductsExcelQuery : IRequest<Result<ExcelExportResult>>
{
    public string? Search { get; set; }
    public bool? CriticalOnly { get; set; }
}
