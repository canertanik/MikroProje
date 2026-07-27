namespace MikroProje.Application.Common.Excel;

public interface IExcelExportService
{
    Task<ExcelExportResult> ExportAsync<T>(IReadOnlyCollection<T> data, string worksheetName, CancellationToken cancellationToken = default);
}
