using MediatR;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.Sales.Queries.ExportSalesExcel;

public class ExportSalesExcelQuery : IRequest<Result<ExcelExportResult>>
{
}
