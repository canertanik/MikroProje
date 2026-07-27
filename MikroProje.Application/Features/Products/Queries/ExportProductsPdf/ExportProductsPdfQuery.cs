using MediatR;
using MikroProje.Application.Common.Pdf;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.Products.Queries.ExportProductsPdf;

public class ExportProductsPdfQuery : IRequest<Result<PdfExportResult>>
{
    public string? Search { get; set; }
    public bool? CriticalOnly { get; set; }
}
