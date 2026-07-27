using MediatR;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.Purchases.Queries.ExportPurchasesExcel;

public class ExportPurchasesExcelQuery : IRequest<Result<ExcelExportResult>>
{
}
