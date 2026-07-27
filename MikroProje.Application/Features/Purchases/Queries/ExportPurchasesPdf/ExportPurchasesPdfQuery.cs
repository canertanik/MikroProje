using MediatR;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.Purchases.Queries.ExportPurchasesPdf;

public class ExportPurchasesPdfQuery : IRequest<Result<PdfExportResult>>
{
}
