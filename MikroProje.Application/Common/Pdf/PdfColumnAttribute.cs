namespace MikroProje.Application.Common.Pdf;

[AttributeUsage(AttributeTargets.Property)]
public class PdfColumnAttribute : Attribute
{
    public string? Header { get; set; }
    public int Order { get; set; }
    public float Width { get; set; } = 1; // Default relative width for QuestPDF
    public string Alignment { get; set; } = "Left"; // Left, Center, Right
    public string? Format { get; set; }
    public bool Ignore { get; set; }
}
