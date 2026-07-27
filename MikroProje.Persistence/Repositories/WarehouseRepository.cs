using Microsoft.EntityFrameworkCore;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Persistence.Contexts;
using MikroProje.Application.Common.Exceptions;

namespace MikroProje.Persistence.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly MikroProjeDbContext _context;

    public WarehouseRepository(MikroProjeDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludedId, CancellationToken cancellationToken)
    {
        return await _context.Warehouses
            .AnyAsync(w => w.Code == code && w.Id != excludedId && !w.IsDeleted, cancellationToken);
    }

    public async Task<Warehouse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == id && !w.IsDeleted, cancellationToken);
    }

    public async Task<Warehouse?> GetDefaultWarehouseAsync(CancellationToken cancellationToken)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(w => w.IsDefault && w.IsActive && !w.IsDeleted, cancellationToken);
    }

    public async Task<PagedResult<Warehouse>> GetAllPagedAsync(string? search, bool? isActive, bool? isDefault, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Warehouses.AsNoTracking().Where(w => !w.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(w => w.Code.Contains(search) || w.Name.Contains(search));
        }

        if (isActive.HasValue)
        {
            query = query.Where(w => w.IsActive == isActive.Value);
        }

        if (isDefault.HasValue)
        {
            query = query.Where(w => w.IsDefault == isDefault.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(w => w.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<Warehouse>.Create(items, pageNumber, pageSize, totalCount);
    }

    public async Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken)
    {
        await _context.Warehouses.AddAsync(warehouse, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasRelatedRecordsAsync(int id, CancellationToken cancellationToken)
    {
        bool hasStocks = await _context.ProductWarehouseStocks.AnyAsync(s => s.WarehouseId == id && s.Quantity > 0 && !s.IsDeleted, cancellationToken);
        if (hasStocks) return true;

        bool hasSales = await _context.Sales.AnyAsync(s => s.WarehouseId == id && !s.IsDeleted, cancellationToken);
        if (hasSales) return true;

        bool hasPurchases = await _context.Purchases.AnyAsync(p => p.WarehouseId == id && !p.IsDeleted, cancellationToken);
        if (hasPurchases) return true;

        bool hasMovements = await _context.StockMovements.AnyAsync(sm => sm.WarehouseId == id && !sm.IsDeleted, cancellationToken);
        if (hasMovements) return true;
        
        bool hasTransfers = await _context.StockTransfers.AnyAsync(t => (t.SourceWarehouseId == id || t.DestinationWarehouseId == id) && !t.IsDeleted, cancellationToken);
        
        return hasTransfers;
    }

    public async Task<Warehouse> UpdateAsync(Warehouse warehouse, byte[] originalRowVersion, CancellationToken cancellationToken)
    {
        try
        {
            _context.Entry(warehouse).Property(w => w.RowVersion).OriginalValue = originalRowVersion;
            warehouse.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return warehouse;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(ex.Message);
        }
    }
}
