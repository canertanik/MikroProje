using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;
using MikroProje.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MikroProje.Persistence.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly MikroProjeDbContext _dbContext;

    public SaleRepository(MikroProjeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Sale?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Sales
            .Include(x => x.CurrentAccount)
            .Include(x => x.Details)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Sale> Items, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Sales
            .AsNoTracking()
            .Include(x => x.CurrentAccount)
            .Include(x => x.Details)
                .ThenInclude(d => d.Product)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.SaleDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyCollection<Sale> Items, int TotalCount)> GetByCurrentAccountAsync(
        int currentAccountId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Sales
            .AsNoTracking()
            .Include(x => x.CurrentAccount)
            .Include(x => x.Details)
                .ThenInclude(d => d.Product)
            .Where(x => x.CurrentAccountId == currentAccountId && !x.IsDeleted)
            .OrderByDescending(x => x.SaleDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Sale> CreateSaleAsync(
        Sale sale,
        List<SaleLineItem> lineItems,
        CurrentAccount currentAccount,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Sale INSERT
            await _dbContext.Sales.AddAsync(sale, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken); // Sale.Id alınır

            // 2. Her satır kalemi için
            foreach (var line in lineItems)
            {
                var product = line.Product;
                var previousQuantity = product.StockQuantity;
                var newQuantity = previousQuantity - line.Quantity;

                // Güvenlik: handler'da kontrol edildi ama burada da check
                if (newQuantity < 0)
                {
                    throw new InvalidOperationException(
                        $"Ürün '{product.Name}' için stok yetersiz. Mevcut: {previousQuantity}, İstenen: {line.Quantity}.");
                }

                var discountRate = line.Discount / 100m;
                var baseAmount = line.UnitPrice * line.Quantity;
                var discountedAmount = baseAmount * (1m - discountRate);
                var lineVat = discountedAmount * (product.VatRate / 100m);
                var lineTotal = discountedAmount + lineVat;

                // 2a. SaleDetail INSERT
                var saleDetail = new SaleDetail
                {
                    SaleId = sale.Id,
                    ProductId = product.Id,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    VatRate = product.VatRate,
                    Discount = line.Discount,
                    LineTotal = Math.Round(lineTotal, 2),
                    CreatedDate = DateTime.UtcNow
                };
                await _dbContext.SaleDetails.AddAsync(saleDetail, cancellationToken);

                // 2b. StockMovement INSERT (StockOut)
                var movement = new StockMovement
                {
                    ProductId = product.Id,
                    MovementType = StockMovementType.StockOut,
                    SourceType = StockMovementSourceType.Sale,
                    Quantity = line.Quantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = newQuantity,
                    DocumentNumber = $"SALE-{sale.Id}",
                    Description = $"Satış No: {sale.Id}",
                    MovementDate = sale.SaleDate,
                    CreatedDate = DateTime.UtcNow
                };
                await _dbContext.StockMovements.AddAsync(movement, cancellationToken);

                // 2c. Product stok güncelle (tracked entity)
                product.StockQuantity = newQuantity;
                product.UpdatedDate = DateTime.UtcNow;
            }

            // 3. CurrentAccount.Balance += GrandTotal
            currentAccount.Balance += sale.GrandTotal;
            currentAccount.UpdatedDate = DateTime.UtcNow;

            // 4. Tüm değişiklikleri kaydet
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // 5. Navigation property'leri doldur (mapper için)
            sale.CurrentAccount = currentAccount;

            return sale;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConcurrencyConflictException(ex.Message);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Sale> UpdateSaleAsync(Sale sale, CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return sale;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(ex.Message);
        }
    }

    public async Task CancelSaleAsync(Sale sale, CancellationToken cancellationToken)
    {
        // İptal için Details + Product + CurrentAccount yüklü olmalı
        // GetByIdAsync zaten hepsini include ediyor — handler bunu kullanıyor
        if (!_dbContext.Entry(sale).Collection(x => x.Details).IsLoaded)
        {
            await _dbContext.Entry(sale).Collection(x => x.Details).LoadAsync(cancellationToken);
            foreach (var d in sale.Details)
            {
                await _dbContext.Entry(d).Reference(x => x.Product).LoadAsync(cancellationToken);
            }
        }

        if (!_dbContext.Entry(sale).Reference(x => x.CurrentAccount).IsLoaded)
        {
            await _dbContext.Entry(sale).Reference(x => x.CurrentAccount).LoadAsync(cancellationToken);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var detail in sale.Details)
            {
                var product = detail.Product;
                var previousQuantity = product.StockQuantity;
                var newQuantity = previousQuantity + detail.Quantity;

                // StockIn (ters hareket)
                var reverseMovement = new StockMovement
                {
                    ProductId = product.Id,
                    MovementType = StockMovementType.StockIn,
                    SourceType = StockMovementSourceType.Return,
                    Quantity = detail.Quantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = newQuantity,
                    DocumentNumber = $"CANCEL-{sale.Id}",
                    Description = $"Satış İptali No: {sale.Id}",
                    MovementDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };
                await _dbContext.StockMovements.AddAsync(reverseMovement, cancellationToken);

                // Product stok geri ekle
                product.StockQuantity = newQuantity;
                product.UpdatedDate = DateTime.UtcNow;
            }

            // CurrentAccount.Balance -= GrandTotal
            sale.CurrentAccount.Balance -= sale.GrandTotal;
            sale.CurrentAccount.UpdatedDate = DateTime.UtcNow;

            // Soft delete
            sale.IsDeleted = true;
            sale.UpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConcurrencyConflictException(ex.Message);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
