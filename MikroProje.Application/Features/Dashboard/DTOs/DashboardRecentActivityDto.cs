namespace MikroProje.Application.Features.Dashboard.DTOs;

public class DashboardRecentActivityDto
{
    public string ActivityType { get; set; } = string.Empty; // "Sale", "Payment", "Purchase", "SupplierPayment", "StockTransfer"
    public string DocumentNumber { get; set; } = string.Empty;
    public string RelatedEntityName { get; set; } = string.Empty; // Cari adı veya Depo adı
    public decimal AmountOrQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}
