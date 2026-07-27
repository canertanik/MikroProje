namespace MikroProje.Application.Common.Excel;

[AttributeUsage(AttributeTargets.Property)]
public class ExcelColumnAttribute : Attribute
{
    public string? Header { get; set; }
    public int Order { get; set; }
    public string? NumberFormat { get; set; }
    public bool Ignore { get; set; }
}
