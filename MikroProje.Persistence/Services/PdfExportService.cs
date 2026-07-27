using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MikroProje.Application.Common.Pdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MikroProje.Persistence.Services;

public class PdfExportService : IPdfExportService
{
    public PdfExportService()
    {
        // Required for QuestPDF Community License
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<PdfExportResult> ExportAsync<T>(IReadOnlyCollection<T> data, string documentTitle, string fileNamePrefix, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var propertiesInfo = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .Select(p => new
            {
                Property = p,
                Attribute = p.GetCustomAttribute<PdfColumnAttribute>()
            })
            .Where(x => x.Attribute == null || !x.Attribute.Ignore)
            .OrderBy(x => x.Attribute?.Order ?? int.MaxValue)
            .ToArray();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                page.Header().Element(headerContainer => ComposeHeader(headerContainer, documentTitle));
                page.Content().Element(contentContainer => ComposeContent(contentContainer, propertiesInfo, data, cancellationToken));
                page.Footer().Element(ComposeFooter);
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);

        var dateSuffix = DateTime.Now.ToString("yyyy-MM-dd");
        var fileName = $"{fileNamePrefix}_{dateSuffix}.pdf";

        return Task.FromResult(new PdfExportResult
        {
            Content = stream.ToArray(),
            FileName = fileName
        });
    }

    private void ComposeHeader(IContainer container, string title)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("MikroProje ERP").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text(title).FontSize(14);
                column.Item().Text($"Oluþturulma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}").FontSize(10);
            });
        });
    }

    private void ComposeContent<T>(IContainer container, dynamic[] propertiesInfo, IReadOnlyCollection<T> data, CancellationToken cancellationToken)
    {
        container.PaddingVertical(1, Unit.Centimetre).Table(table =>
        {
            // Define Columns
            table.ColumnsDefinition(columns =>
            {
                foreach (var prop in propertiesInfo)
                {
                    float width = prop.Attribute?.Width ?? 1f;
                    columns.RelativeColumn(width);
                }
            });

            // Header Row
            table.Header(header =>
            {
                foreach (var prop in propertiesInfo)
                {
                    string headerText = prop.Attribute?.Header ?? prop.Property.Name;
                    var alignment = prop.Attribute?.Alignment?.ToLower() ?? "left";

                    var cell = header.Cell()
                        .Background(Colors.Grey.Lighten2)
                        .BorderBottom(1)
                        .BorderColor(Colors.Black)
                        .Padding(2);
                        
                    var textCell = cell.Text(headerText).SemiBold();
                    
                    if (alignment == "right") cell.AlignRight();
                    else if (alignment == "center") cell.AlignCenter();
                    else cell.AlignLeft();
                }
            });

            // Data Rows
            uint rowIndex = 0;
            foreach (var item in data)
            {
                cancellationToken.ThrowIfCancellationRequested();
                rowIndex++;
                var backgroundColor = rowIndex % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                foreach (var prop in propertiesInfo)
                {
                    var value = prop.Property.GetValue(item);
                    var attr = prop.Attribute;
                    string strValue = FormatValue(value, prop.Property.PropertyType, attr?.Format);
                    var alignment = attr?.Alignment?.ToLower() ?? "left";

                    var cell = table.Cell()
                        .Background(backgroundColor)
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten3)
                        .Padding(2);

                    // Add text
                    var textBlock = cell.Text(strValue);

                    if (alignment == "right") cell.AlignRight();
                    else if (alignment == "center") cell.AlignCenter();
                    else cell.AlignLeft();
                }
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Sayfa ");
            x.CurrentPageNumber();
            x.Span(" / ");
            x.TotalPages();
        });
    }

    private string FormatValue(object? value, Type propertyType, string? format)
    {
        if (value == null)
            return string.Empty;

        var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        if (type == typeof(DateTime))
        {
            var dt = (DateTime)value;
            return dt.ToString(format ?? "dd.MM.yyyy HH:mm", new System.Globalization.CultureInfo("tr-TR"));
        }
        else if (type.Name == "DateOnly")
        {
            return value.ToString();
        }
        else if (type == typeof(bool))
        {
            return ((bool)value) ? "Evet" : "Hayýr";
        }
        else if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
        {
            var d = Convert.ToDecimal(value);
            return d.ToString(format ?? "#,##0.00", new System.Globalization.CultureInfo("tr-TR"));
        }
        else if (type == typeof(int) || type == typeof(long) || type == typeof(short))
        {
            var i = Convert.ToInt64(value);
            return i.ToString(format ?? "#,##0", new System.Globalization.CultureInfo("tr-TR"));
        }

        var str = value.ToString() ?? string.Empty;
        
        // Remove control characters except newline if necessary, but generally PDF handles it.
        // We will just return plain text.
        return str;
    }
}
