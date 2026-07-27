using MediatR;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.Sales.Queries.ExportSalesPdf;

public class ExportSalesPdfQuery : IRequest<Result<PdfExportResult>>
{
}
