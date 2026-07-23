namespace MikroProje.Application.Features.Dashboard.DTOs;

public class DashboardSummaryDto
{
    public int TotalCustomerCount { get; set; }
    public int TotalSupplierCount { get; set; }
    public int TotalProductCount { get; set; }

    public decimal TotalCustomerReceivable { get; set; }
    public decimal TotalSupplierPayable { get; set; }

    public int TotalStockQuantity { get; set; }
    public int LowStockProductCount { get; set; }
    public int OutOfStockProductCount { get; set; }

    public decimal TodaySalesTotal { get; set; }
    public decimal TodayPurchaseTotal { get; set; }

    public decimal ThisMonthSalesTotal { get; set; }
    public decimal ThisMonthPurchaseTotal { get; set; }

    public decimal TodayCustomerPaymentTotal { get; set; }
    public decimal TodaySupplierPaymentTotal { get; set; }
}
