using Microsoft.EntityFrameworkCore;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;
using MikroProje.Persistence.Contexts;

namespace MikroProje.Persistence.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly MikroProjeDbContext _dbContext;

    public PurchaseRepository(MikroProjeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Purchase?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Purchases
            .Include(x => x.CurrentAccount)
            .Include(x => x.Warehouse)
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Purchase> Items, int TotalCount)> GetAllAsync(string? searchTerm, DateTime? startDate, DateTime? endDate, PurchaseStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Purchases
            .AsNoTracking()
            .Include(x => x.CurrentAccount)
            .Include(x => x.Warehouse)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower().Trim();
            
            string termForId = term;
            if (termForId.StartsWith("pur-"))
                termForId = termForId.Substring(4).Trim();
            else if (termForId.StartsWith("pur "))
                termForId = termForId.Substring(4).Trim();
            else if (termForId.StartsWith("pur"))
                termForId = termForId.Substring(3).Trim();
                
            if (termForId.StartsWith("0"))
                termForId = termForId.TrimStart('0');
            
            if (string.IsNullOrEmpty(termForId))
                termForId = term;

            bool isNumericId = int.TryParse(termForId, out int searchId);

            query = query.Where(x => (isNumericId && x.Id == searchId) || 
                                     (x.CurrentAccount != null && x.CurrentAccount.Name.ToLower().Contains(term)) ||
                                     (x.Description != null && x.Description.ToLower().Contains(term)));
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => x.PurchaseDate >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.PurchaseDate <= end);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        query = query.OrderByDescending(x => x.PurchaseDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Purchase> CreatePurchaseAsync(Purchase purchase, List<PurchaseItem> lineItems, CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
            // 1. Purchase INSERT (Status = Pending by default)
            purchase.Status = PurchaseStatus.Pending;
            await _dbContext.Purchases.AddAsync(purchase, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken); // Get the Purchase.Id

            // 2. PurchaseItems INSERT
            foreach (var line in lineItems)
            {
                var purchaseItem = new PurchaseItem
                {
                    PurchaseId = purchase.Id,
                    ProductId = line.ProductId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    VatRate = line.VatRate,
                    LineSubtotal = line.LineSubtotal,
                    VatAmount = line.VatAmount,
                    LineTotal = line.LineTotal,
                    CreatedDate = DateTime.UtcNow
                };
                await _dbContext.PurchaseItems.AddAsync(purchaseItem, cancellationToken);
            }

            // 3. Save and Commit — stok, bakiye ve stok hareketi OLUŞTURULMAZ
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Mapper için property set
            purchase.Items = lineItems;

            return purchase;
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
        });
    }

    public async Task<Purchase> ReceivePurchaseAsync(int purchaseId, CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
            // 1. Purchase yükle (tracking ile)
            var purchase = await _dbContext.Purchases
                .Include(x => x.CurrentAccount)
                .Include(x => x.Warehouse)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(x => x.Id == purchaseId, cancellationToken);

            if (purchase == null)
                throw new KeyNotFoundException($"Satın alma (Id={purchaseId}) bulunamadı.");

            // 2. Durum kontrolü — sadece Pending kabul edilir
            if (purchase.Status != PurchaseStatus.Pending)
                throw new InvalidOperationException(
                    purchase.Status == PurchaseStatus.Received
                        ? "Bu satın alma zaten depoya alınmış. Aynı işlem tekrar yapılamaz."
                        : "İptal edilmiş satın alma için depoya giriş yapılamaz.");

            // 3. Her satın alma kalemi için stok artışı ve StockMovement
            foreach (var item in purchase.Items)
            {
                var product = item.Product;
                var previousQuantity = product.StockQuantity;
                var newQuantity = previousQuantity + item.Quantity;

                // 3a. StockMovement INSERT (StockIn)
                var movement = new StockMovement
                {
                    WarehouseId = purchase.WarehouseId,
                    ProductId = product.Id,
                    MovementType = StockMovementType.StockIn,
                    SourceType = StockMovementSourceType.Purchase,
                    Quantity = item.Quantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = newQuantity,
                    DocumentNumber = $"PURCHASE-{purchase.Id}",
                    Description = $"Alım No: {purchase.Id} - Depoya Giriş",
                    MovementDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };
                await _dbContext.StockMovements.AddAsync(movement, cancellationToken);

                // 3b. Product stock update
                product.StockQuantity = newQuantity;
                product.UpdatedDate = DateTime.UtcNow;

                // 3c. ProductWarehouseStock update
                var warehouseStock = await _dbContext.ProductWarehouseStocks
                    .FirstOrDefaultAsync(s => s.ProductId == product.Id && s.WarehouseId == purchase.WarehouseId && !s.IsDeleted, cancellationToken);
                
                if (warehouseStock == null)
                {
                    warehouseStock = new ProductWarehouseStock
                    {
                        ProductId = product.Id,
                        WarehouseId = purchase.WarehouseId,
                        Quantity = item.Quantity,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    await _dbContext.ProductWarehouseStocks.AddAsync(warehouseStock, cancellationToken);
                }
                else
                {
                    warehouseStock.Quantity += item.Quantity;
                    warehouseStock.UpdatedDate = DateTime.UtcNow;
                }
            }

            // 4. Tedarikçi cari bakiyesi güncelleme
            // Tedarikçiden ürün aldığımızda ona olan borcumuz artar (yani net bakiyesi negatif yönde artar)
            var currentAccount = purchase.CurrentAccount;
            currentAccount.Balance -= purchase.GrandTotal;
            currentAccount.UpdatedDate = DateTime.UtcNow;

            // 5. Purchase durumunu güncelle
            purchase.Status = PurchaseStatus.Received;
            purchase.ReceivedDate = DateTime.UtcNow;
            purchase.UpdatedDate = DateTime.UtcNow;

            // 6. Save and Commit
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

                return purchase;
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
        });
    }

    public async Task CancelPurchaseAsync(int purchaseId, CancellationToken cancellationToken)
    {
        var purchase = await _dbContext.Purchases
            .FirstOrDefaultAsync(x => x.Id == purchaseId, cancellationToken);

        if (purchase == null)
            throw new KeyNotFoundException($"Satın alma (Id={purchaseId}) bulunamadı.");

        if (purchase.Status == PurchaseStatus.Received)
            throw new InvalidOperationException("Depoya alınmış satın alma iptal edilemez. Önce iade işlemi yapılmalıdır.");

        if (purchase.Status == PurchaseStatus.Cancelled)
            throw new InvalidOperationException("Bu satın alma zaten iptal edilmiş.");

        purchase.Status = PurchaseStatus.Cancelled;
        purchase.IsDeleted = true;
        purchase.UpdatedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePurchaseAsync(int purchaseId, CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
            var purchase = await _dbContext.Purchases
                .Include(x => x.CurrentAccount)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(x => x.Id == purchaseId && !x.IsDeleted, cancellationToken);

            if (purchase == null)
                throw new KeyNotFoundException($"Satın alma (Id={purchaseId}) bulunamadı.");

            if (purchase.Status == PurchaseStatus.Received)
            {
                // Revert stocks and balances
                foreach (var item in purchase.Items)
                {
                    var product = item.Product;
                    product.StockQuantity -= item.Quantity;
                    product.UpdatedDate = DateTime.UtcNow;

                    var warehouseStock = await _dbContext.ProductWarehouseStocks
                        .FirstOrDefaultAsync(s => s.ProductId == product.Id && s.WarehouseId == purchase.WarehouseId && !s.IsDeleted, cancellationToken);
                    
                    if (warehouseStock != null)
                    {
                        warehouseStock.Quantity -= item.Quantity;
                        warehouseStock.UpdatedDate = DateTime.UtcNow;
                    }
                }

                // Delete StockMovements for this purchase
                var movements = await _dbContext.StockMovements
                    .Where(m => m.DocumentNumber == $"PURCHASE-{purchase.Id}" && !m.IsDeleted)
                    .ToListAsync(cancellationToken);

                foreach (var m in movements)
                {
                    m.IsDeleted = true;
                    m.UpdatedDate = DateTime.UtcNow;
                }

                var currentAccount = purchase.CurrentAccount;
                currentAccount.Balance += purchase.GrandTotal;
                currentAccount.UpdatedDate = DateTime.UtcNow;
            }

            purchase.IsDeleted = true;
            purchase.UpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
            }
        });
    }

    public async Task<List<MikroProje.Application.Features.SupplierStatements.DTOs.SupplierStatementItemDto>> GetStatementPurchasesAsync(
        int currentAccountId, CancellationToken cancellationToken)
    {
        return await _dbContext.Purchases
            .AsNoTracking()
            .Where(x => x.CurrentAccountId == currentAccountId && !x.IsDeleted && x.Status == PurchaseStatus.Received)
            .Select(x => new MikroProje.Application.Features.SupplierStatements.DTOs.SupplierStatementItemDto
            {
                Date = x.ReceivedDate ?? x.PurchaseDate,
                DocumentType = DocumentType.Purchase,
                DocumentId = x.Id,
                Description = x.Description ?? string.Empty,
                Debit = x.GrandTotal,
                Credit = 0,
                RunningBalance = 0
            })
            .ToListAsync(cancellationToken);
    }
}
