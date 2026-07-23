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

    public async Task<DashboardSummaryDto> GetSummaryAsync(DateTime todayStart, DateTime tomorrowStart, DateTime monthStart, DateTime nextMonthStart, CancellationToken cancellationToken)
    {
        var dto = new DashboardSummaryDto();

        // 1. Customer Stats
        var customerStats = await _dbContext.CurrentAccounts
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Type == CurrentAccountType.Customer)
            .GroupBy(x => 1)
            .Select(g => new
            {
                Count = g.Count(),
                TotalReceivable = g.Sum(x => (decimal?)x.Balance) ?? 0
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (customerStats != null)
        {
            dto.TotalCustomerCount = customerStats.Count;
            dto.TotalCustomerReceivable = customerStats.TotalReceivable;
        }

        // 2. Supplier Stats
        var supplierStats = await _dbContext.CurrentAccounts
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Type == CurrentAccountType.Supplier)
            .GroupBy(x => 1)
            .Select(g => new
            {
                Count = g.Count(),
                TotalPayable = g.Sum(x => (decimal?)x.Balance) ?? 0
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (supplierStats != null)
        {
            dto.TotalSupplierCount = supplierStats.Count;
            dto.TotalSupplierPayable = supplierStats.TotalPayable;
        }

        // 3. Product Stats
        var productStats = await _dbContext.Products
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalCount = g.Count(),
                TotalStock = g.Sum(x => (int?)x.StockQuantity) ?? 0,
                LowStock = g.Count(x => x.StockQuantity > 0 && x.StockQuantity <= x.CriticalStockQuantity),
                OutOfStock = g.Count(x => x.StockQuantity <= 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (productStats != null)
        {
            dto.TotalProductCount = productStats.TotalCount;
            dto.TotalStockQuantity = productStats.TotalStock;
            dto.LowStockProductCount = productStats.LowStock;
            dto.OutOfStockProductCount = productStats.OutOfStock;
        }

        // 4. Sale Stats
        var saleStats = await _dbContext.Sales
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.SaleDate >= monthStart && x.SaleDate < nextMonthStart)
            .GroupBy(x => 1)
            .Select(g => new
            {
                ThisMonthSales = g.Sum(x => (decimal?)x.GrandTotal) ?? 0,
                TodaySales = g.Where(x => x.SaleDate >= todayStart && x.SaleDate < tomorrowStart).Sum(x => (decimal?)x.GrandTotal) ?? 0
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (saleStats != null)
        {
            dto.ThisMonthSalesTotal = saleStats.ThisMonthSales;
            dto.TodaySalesTotal = saleStats.TodaySales;
        }

        // 5. Purchase Stats
        var purchaseStats = await _dbContext.Purchases
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.PurchaseDate >= monthStart && x.PurchaseDate < nextMonthStart)
            .GroupBy(x => 1)
            .Select(g => new
            {
                ThisMonthPurchases = g.Sum(x => (decimal?)x.GrandTotal) ?? 0,
                TodayPurchases = g.Where(x => x.PurchaseDate >= todayStart && x.PurchaseDate < tomorrowStart).Sum(x => (decimal?)x.GrandTotal) ?? 0
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (purchaseStats != null)
        {
            dto.ThisMonthPurchaseTotal = purchaseStats.ThisMonthPurchases;
            dto.TodayPurchaseTotal = purchaseStats.TodayPurchases;
        }

        // 6. Today Customer Payments
        dto.TodayCustomerPaymentTotal = await _dbContext.Payments
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.PaymentDate >= todayStart && x.PaymentDate < tomorrowStart)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;

        // 7. Today Supplier Payments
        dto.TodaySupplierPaymentTotal = await _dbContext.SupplierPayments
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.PaymentDate >= todayStart && x.PaymentDate < tomorrowStart)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;

        return dto;
    }
}
