using ClosedXML.Excel;
using MikroProje.Application.Common.Excel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MikroProje.Persistence.Services;

public class ExcelExportService : IExcelExportService
{
    public Task<ExcelExportResult> ExportAsync<T>(IReadOnlyCollection<T> data, string worksheetName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook();
        
        // Clean worksheet name
        var cleanName = Regex.Replace(worksheetName, @"[\\/?*[\]]", "");
        if (cleanName.Length > 31)
            cleanName = cleanName.Substring(0, 31);
        if (string.IsNullOrWhiteSpace(cleanName))
            cleanName = "Sheet1";

        var worksheet = workbook.Worksheets.Add(cleanName);

        // Get properties and attributes
        var propertiesInfo = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .Select(p => new
            {
                Property = p,
                Attribute = p.GetCustomAttribute<ExcelColumnAttribute>()
            })
            .Where(x => x.Attribute == null || !x.Attribute.Ignore)
            .OrderBy(x => x.Attribute?.Order ?? int.MaxValue)
            .ToArray();

        // Headers
        for (var i = 0; i < propertiesInfo.Length; i++)
        {
            var header = propertiesInfo[i].Attribute?.Header ?? propertiesInfo[i].Property.Name;
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        // Data
        var row = 2;
        foreach (var item in data)
        {
            cancellationToken.ThrowIfCancellationRequested();

            for (var i = 0; i < propertiesInfo.Length; i++)
            {
                var propInfo = propertiesInfo[i].Property;
                var attr = propertiesInfo[i].Attribute;
                var value = propInfo.GetValue(item);
                var cell = worksheet.Cell(row, i + 1);

                if (value == null)
                {
                    cell.Value = string.Empty;
                    continue;
                }

                var type = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;

                if (type == typeof(DateTime))
                {
                    cell.Value = (DateTime)value;
                    cell.Style.DateFormat.Format = attr?.NumberFormat ?? "dd.MM.yyyy HH:mm";
                }
                else if (type.Name == "DateOnly") // For .NET 6+ DateOnly
                {
                    cell.Value = value.ToString();
                }
                else if (type == typeof(bool))
                {
                    cell.Value = ((bool)value) ? "Evet" : "Hayýr";
                }
                else if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
                {
                    cell.Value = Convert.ToDecimal(value);
                    cell.Style.NumberFormat.Format = attr?.NumberFormat ?? "#,##0.00";
                }
                else if (type == typeof(int) || type == typeof(long) || type == typeof(short))
                {
                    cell.Value = Convert.ToDouble(value);
                    cell.Style.NumberFormat.Format = attr?.NumberFormat ?? "#,##0";
                }
                else
                {
                    var strValue = value.ToString() ?? string.Empty;
                    // Formula Injection protection
                    if (strValue.StartsWith("=") || strValue.StartsWith("+") || strValue.StartsWith("-") || strValue.StartsWith("@"))
                    {
                        strValue = "'" + strValue;
                    }
                    cell.Value = strValue;
                }
            }
            row++;
        }

        if (propertiesInfo.Length > 0)
        {
            // AutoFilter & Freeze Panes
            worksheet.Range(1, 1, row - 1, propertiesInfo.Length).SetAutoFilter();
            worksheet.SheetView.FreezeRows(1);

            // AutoFit with Max Width
            worksheet.Columns(1, propertiesInfo.Length).AdjustToContents();
            for (int i = 1; i <= propertiesInfo.Length; i++)
            {
                if (worksheet.Column(i).Width > 50)
                {
                    worksheet.Column(i).Width = 50;
                }
            }
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var dateSuffix = DateTime.Now.ToString("yyyy-MM-dd");
        var fileName = $"{cleanName}_{dateSuffix}.xlsx";

        return Task.FromResult(new ExcelExportResult
        {
            Content = stream.ToArray(),
            FileName = fileName
        });
    }
}
