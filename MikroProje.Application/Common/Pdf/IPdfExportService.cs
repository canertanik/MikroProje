using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MikroProje.Application.Common.Pdf;

public interface IPdfExportService
{
    Task<PdfExportResult> ExportAsync<T>(
        IReadOnlyCollection<T> data,
        string documentTitle,
        string fileNamePrefix,
        CancellationToken cancellationToken = default);
}
