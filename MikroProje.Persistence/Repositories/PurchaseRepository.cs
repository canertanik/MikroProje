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
            .Include(x => x.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Purchase> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Purchases
            .AsNoTracking()
            .Include(x => x.CurrentAccount)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.PurchaseDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Purchase> CreatePurchaseAsync(Purchase purchase, List<PurchaseItem> lineItems, CurrentAccount currentAccount, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // 1. Purchase INSERT
            await _dbContext.Purchases.AddAsync(purchase, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken); // Get the Purchase.Id

            // 2. Process each item
            foreach (var line in lineItems)
            {
                var product = line.Product;
                var previousQuantity = product.StockQuantity;
                var newQuantity = previousQuantity + line.Quantity;

                // 2a. PurchaseItem INSERT
                var purchaseItem = new PurchaseItem
                {
                    PurchaseId = purchase.Id,
                    ProductId = product.Id,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    VatRate = line.VatRate,
                    LineSubtotal = line.LineSubtotal,
                    VatAmount = line.VatAmount,
                    LineTotal = line.LineTotal,
                    CreatedDate = DateTime.UtcNow
                };
                await _dbContext.PurchaseItems.AddAsync(purchaseItem, cancellationToken);

                // 2b. StockMovement INSERT (StockIn)
                var movement = new StockMovement
                {
                    WarehouseId = purchase.WarehouseId, // FIX: assign WarehouseId
                    ProductId = product.Id,
                    MovementType = StockMovementType.StockIn,
                    SourceType = StockMovementSourceType.Purchase,
                    Quantity = line.Quantity,
                    PreviousQuantity = previousQuantity,
                    NewQuantity = newQuantity,
                    DocumentNumber = $"PURCHASE-{purchase.Id}",
                    Description = $"Alım No: {purchase.Id}",
                    MovementDate = purchase.PurchaseDate,
                    CreatedDate = DateTime.UtcNow
                };
                await _dbContext.StockMovements.AddAsync(movement, cancellationToken);

                // 2c. Product stock update
                product.StockQuantity = newQuantity;
                product.UpdatedDate = DateTime.UtcNow;
            }

            // 3. CurrentAccount Balance Update
            // Tedarikçiden ürün aldığımızda ona olan borcumuz artar, bu da bakiyenin artması demektir.
            currentAccount.Balance += purchase.GrandTotal;
            currentAccount.UpdatedDate = DateTime.UtcNow;

            // 4. Save and Commit
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Mapper için property set
            purchase.CurrentAccount = currentAccount;
            purchase.Items = lineItems; // Items'ları set ediyoruz ki mapper okuyabilsin

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
    }

    public async Task<List<MikroProje.Application.Features.SupplierStatements.DTOs.SupplierStatementItemDto>> GetStatementPurchasesAsync(
        int currentAccountId, CancellationToken cancellationToken)
    {
        return await _dbContext.Purchases
            .AsNoTracking()
            .Where(x => x.CurrentAccountId == currentAccountId && !x.IsDeleted)
            .Select(x => new MikroProje.Application.Features.SupplierStatements.DTOs.SupplierStatementItemDto
            {
                Date = x.PurchaseDate,
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
