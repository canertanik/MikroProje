using MediatR;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.CurrentAccounts.Queries.ExportCurrentAccountsPdf;

public class ExportCurrentAccountsPdfQuery : IRequest<Result<PdfExportResult>>
{
}
