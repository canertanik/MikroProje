namespace MikroProje.Application.Features.Dashboard.DTOs;

public class DashboardSummaryDto
{
    public int TotalCustomerCount { get; set; }
    public int TotalSupplierCount { get; set; }
    public int ActiveProductCount { get; set; }
    public int CriticalStockProductCount { get; set; }

    public decimal TotalCustomerReceivable { get; set; }
    public decimal TotalSupplierPayable { get; set; }

    public decimal SalesTotal { get; set; }
    public decimal PurchasesTotal { get; set; }

    public decimal CustomerPaymentTotal { get; set; }
    public decimal SupplierPaymentTotal { get; set; }
    public decimal NetCashFlow => CustomerPaymentTotal - SupplierPaymentTotal;

    public int PendingPurchaseCount { get; set; }
    public int DraftTransferCount { get; set; }
    public int CustomerWithDebtCount { get; set; }
    public int SupplierWithDebtCount { get; set; }
}
