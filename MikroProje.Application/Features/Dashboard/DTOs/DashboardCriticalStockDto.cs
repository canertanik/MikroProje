namespace MikroProje.Application.Features.Dashboard.DTOs;

public class DashboardCriticalStockDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int CriticalStock { get; set; }
    public string Status { get; set; } = string.Empty;
}
