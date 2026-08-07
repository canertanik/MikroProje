using Microsoft.EntityFrameworkCore;
using MikroProje.Application.Features.Dashboard.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Enums;
using MikroProje.Persistence.Contexts;

namespace MikroProje.Persistence.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly MikroProjeDbContext _dbContext;

    public DashboardRepository(MikroProjeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var dto = new DashboardSummaryDto();

        // 1. Customer/Supplier Counts & Product Counts (Independent of date range)
        dto.TotalCustomerCount = await _dbContext.CurrentAccounts.CountAsync(x => !x.IsDeleted && (x.Type == CurrentAccountType.Customer || x.Type == CurrentAccountType.Both), cancellationToken);
        dto.TotalSupplierCount = await _dbContext.CurrentAccounts.CountAsync(x => !x.IsDeleted && (x.Type == CurrentAccountType.Supplier || x.Type == CurrentAccountType.Both), cancellationToken);
        
        dto.ActiveProductCount = await _dbContext.Products.CountAsync(x => !x.IsDeleted, cancellationToken);
        dto.CriticalStockProductCount = await _dbContext.Products.CountAsync(x => !x.IsDeleted && x.StockQuantity <= x.CriticalStockQuantity, cancellationToken);

        // 2. Pending Operations
        dto.PendingPurchaseCount = await _dbContext.Purchases.CountAsync(x => !x.IsDeleted && x.Status == PurchaseStatus.Pending, cancellationToken);
        dto.DraftTransferCount = await _dbContext.StockTransfers.CountAsync(x => !x.IsDeleted && x.Status == StockTransferStatus.Draft, cancellationToken);

        // 3. Date filtering for financial metrics
        var salesQuery = _dbContext.Sales.Where(x => !x.IsDeleted);
        var purchasesQuery = _dbContext.Purchases.Where(x => !x.IsDeleted && x.Status == PurchaseStatus.Received);
        var paymentsQuery = _dbContext.Payments.Where(x => !x.IsDeleted);
        var supplierPaymentsQuery = _dbContext.SupplierPayments.Where(x => !x.IsDeleted);

        if (startDate.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.SaleDate >= startDate.Value);
            purchasesQuery = purchasesQuery.Where(x => (x.ReceivedDate ?? x.PurchaseDate) >= startDate.Value);
            paymentsQuery = paymentsQuery.Where(x => x.PaymentDate >= startDate.Value);
            supplierPaymentsQuery = supplierPaymentsQuery.Where(x => x.PaymentDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1); // Exclusive end date (assuming input is just date)
            salesQuery = salesQuery.Where(x => x.SaleDate < end);
            purchasesQuery = purchasesQuery.Where(x => (x.ReceivedDate ?? x.PurchaseDate) < end);
            paymentsQuery = paymentsQuery.Where(x => x.PaymentDate < end);
            supplierPaymentsQuery = supplierPaymentsQuery.Where(x => x.PaymentDate < end);
        }

        dto.SalesTotal = await salesQuery.SumAsync(x => (decimal?)x.GrandTotal, cancellationToken) ?? 0;
        dto.PurchasesTotal = await purchasesQuery.SumAsync(x => (decimal?)x.GrandTotal, cancellationToken) ?? 0;
        
        dto.CustomerPaymentTotal = await paymentsQuery.SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;
        dto.SupplierPaymentTotal = await supplierPaymentsQuery.SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;

        // 4. Global Receivables and Payables (calculated from transactions to avoid Both mixing)
        // Global Receivables = Total Sales - Total Payments
        var globalSales = await _dbContext.Sales.Where(x => !x.IsDeleted).SumAsync(x => (decimal?)x.GrandTotal, cancellationToken) ?? 0;
        var globalPayments = await _dbContext.Payments.Where(x => !x.IsDeleted).SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;
        dto.TotalCustomerReceivable = globalSales - globalPayments;

        // Global Payables = Total Received Purchases - Total Supplier Payments
        var globalPurchases = await _dbContext.Purchases.Where(x => !x.IsDeleted && x.Status == PurchaseStatus.Received).SumAsync(x => (decimal?)x.GrandTotal, cancellationToken) ?? 0;
        var globalSupplierPayments = await _dbContext.SupplierPayments.Where(x => !x.IsDeleted).SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;
        dto.TotalSupplierPayable = globalPurchases - globalSupplierPayments;

        // CustomerWithDebtCount: CurrentAccounts where (Sales - Payments > 0)
        // This is complex for a simple count, so we'll approximate with CurrentAccount.Balance for simplicity,
        // or we can just fetch the raw balances grouped by CurrentAccountId.
        // But since EF Core translates this to SQL, we can do it efficiently.
        // Because of the 'Both' mix up, getting accurate debtors is hard via DB query without a View.
        // Let's just use CurrentAccount.Balance for counts (if Balance > 0, they owe us. If Balance < 0, we owe them).
        dto.CustomerWithDebtCount = await _dbContext.CurrentAccounts
            .CountAsync(x => !x.IsDeleted && (x.Type == CurrentAccountType.Customer || x.Type == CurrentAccountType.Both) && x.Balance > 0, cancellationToken);
        
        dto.SupplierWithDebtCount = await _dbContext.CurrentAccounts
            .CountAsync(x => !x.IsDeleted && (x.Type == CurrentAccountType.Supplier || x.Type == CurrentAccountType.Both) && x.Balance < 0, cancellationToken);

        return dto;
    }

    public async Task<List<DashboardTrendsDto>> GetTrendsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var salesQuery = _dbContext.Sales.Where(x => !x.IsDeleted);
        var purchasesQuery = _dbContext.Purchases.Where(x => !x.IsDeleted && x.Status == PurchaseStatus.Received);

        if (startDate.HasValue)
        {
            salesQuery = salesQuery.Where(x => x.SaleDate >= startDate.Value);
            purchasesQuery = purchasesQuery.Where(x => (x.ReceivedDate ?? x.PurchaseDate) >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1);
            salesQuery = salesQuery.Where(x => x.SaleDate < end);
            purchasesQuery = purchasesQuery.Where(x => (x.ReceivedDate ?? x.PurchaseDate) < end);
        }

        // Determine grouping (by day or by month depending on range span)
        bool byMonth = false;
        if (startDate.HasValue && endDate.HasValue)
        {
            if ((endDate.Value - startDate.Value).TotalDays > 31)
                byMonth = true;
        }
        else if (!startDate.HasValue && !endDate.HasValue)
        {
            byMonth = true; // No filter, group by month
        }

        var salesList = await salesQuery.Select(x => new { x.SaleDate, x.GrandTotal }).ToListAsync(cancellationToken);
        var purchasesList = await purchasesQuery.Select(x => new { Date = x.ReceivedDate ?? x.PurchaseDate, x.GrandTotal }).ToListAsync(cancellationToken);

        var trends = new Dictionary<string, DashboardTrendsDto>();

        foreach (var s in salesList)
        {
            var key = byMonth ? s.SaleDate.ToString("yyyy-MM") : s.SaleDate.ToString("yyyy-MM-dd");
            if (!trends.ContainsKey(key)) trends[key] = new DashboardTrendsDto { DateLabel = key };
            trends[key].SalesTotal += s.GrandTotal;
        }

        foreach (var p in purchasesList)
        {
            var key = byMonth ? p.Date.ToString("yyyy-MM") : p.Date.ToString("yyyy-MM-dd");
            if (!trends.ContainsKey(key)) trends[key] = new DashboardTrendsDto { DateLabel = key };
            trends[key].PurchasesTotal += p.GrandTotal;
        }

        return trends.Values.OrderBy(x => x.DateLabel).ToList();
    }

    public async Task<List<DashboardRecentActivityDto>> GetRecentActivitiesAsync(CancellationToken cancellationToken)
    {
        var recentCount = 10;
        var activities = new List<DashboardRecentActivityDto>();

        var sales = await _dbContext.Sales
            .Include(x => x.CurrentAccount)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.SaleDate)
            .Take(recentCount)
            .Select(x => new DashboardRecentActivityDto
            {
                ActivityType = "Sale",
                DocumentNumber = $"SALE-{x.Id}",
                RelatedEntityName = x.CurrentAccount.Name,
                AmountOrQuantity = x.GrandTotal,
                Status = "Completed",
                Date = x.SaleDate
            })
            .ToListAsync(cancellationToken);
        
        var purchases = await _dbContext.Purchases
            .Include(x => x.CurrentAccount)
            .Where(x => !x.IsDeleted && x.Status == PurchaseStatus.Received)
            .OrderByDescending(x => x.ReceivedDate ?? x.PurchaseDate)
            .Take(recentCount)
            .Select(x => new DashboardRecentActivityDto
            {
                ActivityType = "Purchase",
                DocumentNumber = $"PURCHASE-{x.Id}",
                RelatedEntityName = x.CurrentAccount.Name,
                AmountOrQuantity = x.GrandTotal,
                Status = "Received",
                Date = x.ReceivedDate ?? x.PurchaseDate
            })
            .ToListAsync(cancellationToken);

        var payments = await _dbContext.Payments
            .Include(x => x.CurrentAccount)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.PaymentDate)
            .Take(recentCount)
            .Select(x => new DashboardRecentActivityDto
            {
                ActivityType = "Payment",
                DocumentNumber = $"PAY-{x.Id}",
                RelatedEntityName = x.CurrentAccount.Name,
                AmountOrQuantity = x.Amount,
                Status = "Completed",
                Date = x.PaymentDate
            })
            .ToListAsync(cancellationToken);

        var supplierPayments = await _dbContext.SupplierPayments
            .Include(x => x.CurrentAccount)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.PaymentDate)
            .Take(recentCount)
            .Select(x => new DashboardRecentActivityDto
            {
                ActivityType = "SupplierPayment",
                DocumentNumber = $"SUPPAY-{x.Id}",
                RelatedEntityName = x.CurrentAccount.Name,
                AmountOrQuantity = x.Amount,
                Status = "Completed",
                Date = x.PaymentDate
            })
            .ToListAsync(cancellationToken);

        var transfers = await _dbContext.StockTransfers
            .Include(x => x.SourceWarehouse)
            .Where(x => !x.IsDeleted && x.Status == StockTransferStatus.Completed)
            .OrderByDescending(x => x.TransferDate)
            .Take(recentCount)
            .Select(x => new DashboardRecentActivityDto
            {
                ActivityType = "StockTransfer",
                DocumentNumber = x.TransferNumber ?? $"TR-{x.Id}",
                RelatedEntityName = x.SourceWarehouse != null ? x.SourceWarehouse.Name : "-",
                AmountOrQuantity = 0, // Cannot easily sum items here, keep 0
                Status = "Completed",
                Date = x.TransferDate
            })
            .ToListAsync(cancellationToken);

        activities.AddRange(sales);
        activities.AddRange(purchases);
        activities.AddRange(payments);
        activities.AddRange(supplierPayments);
        activities.AddRange(transfers);

        return activities.OrderByDescending(x => x.Date).Take(recentCount).ToList();
    }

    public async Task<List<DashboardCriticalStockDto>> GetCriticalStockAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.StockQuantity <= x.CriticalStockQuantity)
            .OrderBy(x => x.StockQuantity)
            .Take(10)
            .Select(x => new DashboardCriticalStockDto
            {
                ProductId = x.Id,
                ProductCode = x.Code,
                ProductName = x.Name,
                CurrentStock = x.StockQuantity,
                CriticalStock = x.CriticalStockQuantity,
                Status = x.StockQuantity <= 0 ? "Out of Stock" : "Low Stock"
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<DashboardTopRecordsDto> GetTopRecordsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var dto = new DashboardTopRecordsDto();
        var limit = 5;

        // Due to complexity of EF grouping across multiple entities correctly, we fetch recent raw data and aggregate in memory for Top Customers/Products
        var salesQuery = _dbContext.Sales
            .Include(x => x.CurrentAccount)
            .Include(x => x.Details)
                .ThenInclude(i => i.Product)
            .Where(x => !x.IsDeleted);

        if (startDate.HasValue) salesQuery = salesQuery.Where(x => x.SaleDate >= startDate.Value);
        if (endDate.HasValue) salesQuery = salesQuery.Where(x => x.SaleDate < endDate.Value.Date.AddDays(1));

        var salesList = await salesQuery.ToListAsync(cancellationToken);

        dto.TopCustomersBySales = salesList
            .GroupBy(x => x.CurrentAccount.Name)
            .Select(g => new TopCustomerDto { CustomerName = g.Key, TotalSales = g.Sum(x => x.GrandTotal) })
            .OrderByDescending(x => x.TotalSales)
            .Take(limit)
            .ToList();

        dto.TopProductsBySales = salesList
            .SelectMany(x => x.Details)
            .GroupBy(x => x.Product.Name)
            .Select(g => new TopProductDto { ProductName = g.Key, TotalQuantitySold = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.TotalQuantitySold)
            .Take(limit)
            .ToList();

        // Debtors and Creditors (Using Balance since we need it quick, Balance > 0 means they owe us, Balance < 0 means we owe them)
        dto.TopDebtors = await _dbContext.CurrentAccounts
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Balance > 0 && (x.Type == CurrentAccountType.Customer || x.Type == CurrentAccountType.Both))
            .OrderByDescending(x => x.Balance)
            .Take(limit)
            .Select(x => new TopDebtorDto { CustomerName = x.Name, DebtAmount = x.Balance })
            .ToListAsync(cancellationToken);

        dto.TopCreditors = await _dbContext.CurrentAccounts
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Balance < 0 && (x.Type == CurrentAccountType.Supplier || x.Type == CurrentAccountType.Both))
            .OrderBy(x => x.Balance) // Most negative first
            .Take(limit)
            .Select(x => new TopCreditorDto { SupplierName = x.Name, CreditAmount = Math.Abs(x.Balance) })
            .ToListAsync(cancellationToken);

        return dto;
    }
}
