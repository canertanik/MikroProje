using MediatR;
using MikroProje.Application.Common.Excel;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.CurrentAccounts.Queries.ExportCurrentAccountsExcel;

public class ExportCurrentAccountsExcelQuery : IRequest<Result<ExcelExportResult>>
{
}
