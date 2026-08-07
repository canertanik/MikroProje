namespace MikroProje.Application.Features.Dashboard.DTOs;

public class DashboardTrendsDto
{
    public string DateLabel { get; set; } = string.Empty;
    public decimal SalesTotal { get; set; }
    public decimal PurchasesTotal { get; set; }
}
