using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;
using MikroProje.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MikroProje.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly MikroProjeDbContext _dbContext;

    public ProductRepository(MikroProjeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludedId, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Code == code && (!excludedId.HasValue || x.Id != excludedId.Value), cancellationToken);
    }

    public async Task<bool> BarcodeExistsAsync(string? barcode, int? excludedId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return false;
        }

        return await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(x => x.Barcode == barcode && (!excludedId.HasValue || x.Id != excludedId.Value), cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<List<Product>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        return await _dbContext.Products
            .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Product> Items, int TotalCount)> GetPagedAsync(string? search, bool? criticalOnly, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Code.Contains(search) || x.Name.Contains(search) || (x.Barcode != null && x.Barcode.Contains(search)));
        }

        if (criticalOnly == true)
        {
            query = query.Where(x => x.StockQuantity <= x.CriticalStockQuantity);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyCollection<Product> Items, int TotalCount)> GetCriticalStockPagedAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await GetPagedAsync(search, true, pageNumber, pageSize, cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Product> CreateWithInitialStockAsync(Product product, int initialStockQuantity, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await _dbContext.Products.AddAsync(product, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (initialStockQuantity > 0)
            {
                var movement = new StockMovement
                {
                    ProductId = product.Id,
                    MovementType = StockMovementType.StockIn,
                    SourceType = StockMovementSourceType.Manual,
                    Quantity = initialStockQuantity,
                    PreviousQuantity = 0,
                    NewQuantity = initialStockQuantity,
                    Description = "Initial stock",
                    MovementDate = DateTime.UtcNow
                };

                product.StockQuantity = initialStockQuantity;
                product.UpdatedDate = DateTime.UtcNow;

                await _dbContext.StockMovements.AddAsync(movement, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return product;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConcurrencyConflictException(exception.Message);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteSoftAsync(Product product, CancellationToken cancellationToken)
    {
        product.IsDeleted = true;
        product.UpdatedDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}