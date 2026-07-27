using Microsoft.EntityFrameworkCore;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Features.StockTransfers.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;
using MikroProje.Persistence.Contexts;

namespace MikroProje.Persistence.Repositories;

public class StockTransferRepository : IStockTransferRepository
{
    private readonly MikroProjeDbContext _context;

    public StockTransferRepository(MikroProjeDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateTransferNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var prefix = "ST-" + today.ToString("yyyyMMdd") + "-";
        
        var maxNumber = await _context.StockTransfers
            .Where(x => x.TransferNumber.StartsWith(prefix))
            .OrderByDescending(x => x.TransferNumber)
            .Select(x => x.TransferNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrEmpty(maxNumber))
        {
            return prefix + "000001";
        }

        var sequenceStr = maxNumber.Substring(prefix.Length);
        if (int.TryParse(sequenceStr, out int sequence))
        {
            return prefix + (sequence + 1).ToString("D6");
        }

        return prefix + "000001";
    }

    public async Task<StockTransferDto?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.StockTransfers
            .AsNoTracking()
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .Include(t => t.Items)
                .ThenInclude(i => i.Product)
            .Where(t => t.Id == id && !t.IsDeleted)
            .Select(t => new StockTransferDto
            {
                Id = t.Id,
                TransferNumber = t.TransferNumber,
                SourceWarehouseId = t.SourceWarehouseId,
                SourceWarehouseCode = t.SourceWarehouse.Code,
                SourceWarehouseName = t.SourceWarehouse.Name,
                DestinationWarehouseId = t.DestinationWarehouseId,
                DestinationWarehouseCode = t.DestinationWarehouse.Code,
                DestinationWarehouseName = t.DestinationWarehouse.Name,
                TransferDate = t.TransferDate,
                Description = t.Description,
                Status = t.Status,
                CreatedDate = t.CreatedDate,
                UpdatedDate = t.UpdatedDate,
                RowVersion = t.RowVersion,
                Items = t.Items.Where(i => !i.IsDeleted).Select(i => new StockTransferItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductCode = i.Product.Code,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<StockTransferListDto>> GetAllPagedAsync(string? search, int? sourceWarehouseId, int? destinationWarehouseId, StockTransferStatus? status, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.StockTransfers
            .AsNoTracking()
            .Include(t => t.SourceWarehouse)
            .Include(t => t.DestinationWarehouse)
            .Where(t => !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.TransferNumber.Contains(search) || 
                                     t.SourceWarehouse.Code.Contains(search) || t.SourceWarehouse.Name.Contains(search) ||
                                     t.DestinationWarehouse.Code.Contains(search) || t.DestinationWarehouse.Name.Contains(search));
        }

        if (sourceWarehouseId.HasValue)
        {
            query = query.Where(t => t.SourceWarehouseId == sourceWarehouseId.Value);
        }

        if (destinationWarehouseId.HasValue)
        {
            query = query.Where(t => t.DestinationWarehouseId == destinationWarehouseId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransferDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransferDate <= endDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new StockTransferListDto
            {
                Id = t.Id,
                TransferNumber = t.TransferNumber,
                SourceWarehouseCode = t.SourceWarehouse.Code,
                DestinationWarehouseCode = t.DestinationWarehouse.Code,
                TransferDate = t.TransferDate,
                Status = t.Status,
                CreatedDate = t.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return PagedResult<StockTransferListDto>.Create(items, pageNumber, pageSize, totalCount);
    }

    public async Task AddAsync(StockTransfer stockTransfer, CancellationToken cancellationToken)
    {
        await _context.StockTransfers.AddAsync(stockTransfer, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteTransferAsync(int transferId, byte[] rowVersion, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var transfer = await _context.StockTransfers
                .Include(t => t.SourceWarehouse)
                .Include(t => t.DestinationWarehouse)
                .Include(t => t.Items)
                .FirstOrDefaultAsync(t => t.Id == transferId && !t.IsDeleted, cancellationToken);

            if (transfer == null)
            {
                throw new InvalidOperationException("Transfer bulunamadı.");
            }

            if (transfer.Status != StockTransferStatus.Draft)
            {
                throw new InvalidOperationException("Sadece taslak (Draft) statüsündeki transferler tamamlanabilir.");
            }

            if (!transfer.SourceWarehouse.IsActive || transfer.SourceWarehouse.IsDeleted ||
                !transfer.DestinationWarehouse.IsActive || transfer.DestinationWarehouse.IsDeleted)
            {
                throw new InvalidOperationException("Kaynak veya hedef depo aktif değil.");
            }

            _context.Entry(transfer).Property(t => t.RowVersion).OriginalValue = rowVersion;

            foreach (var item in transfer.Items.Where(i => !i.IsDeleted))
            {
                // Source PWS
                var sourceStock = await _context.ProductWarehouseStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == transfer.SourceWarehouseId && !s.IsDeleted, cancellationToken);

                if (sourceStock == null || sourceStock.Quantity < item.Quantity)
                {
                    throw new InvalidOperationException("Kaynak depoda yeterli stok bulunmuyor. (ProductId: " + item.ProductId + ")");
                }

                var previousSourceStock = sourceStock.Quantity;
                sourceStock.Quantity -= item.Quantity;
                sourceStock.UpdatedDate = DateTime.UtcNow;

                // Destination PWS
                var destStock = await _context.ProductWarehouseStocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId && s.WarehouseId == transfer.DestinationWarehouseId && !s.IsDeleted, cancellationToken);

                var previousDestStock = 0;
                if (destStock == null)
                {
                    destStock = new ProductWarehouseStock
                    {
                        ProductId = item.ProductId,
                        WarehouseId = transfer.DestinationWarehouseId,
                        Quantity = item.Quantity,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    await _context.ProductWarehouseStocks.AddAsync(destStock, cancellationToken);
                }
                else
                {
                    previousDestStock = destStock.Quantity;
                    destStock.Quantity += item.Quantity;
                    destStock.UpdatedDate = DateTime.UtcNow;
                }

                // TransferOut Movement
                var transferOut = new StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = transfer.SourceWarehouseId,
                    MovementType = StockMovementType.TransferOut,
                    SourceType = StockMovementSourceType.StockTransfer,
                    DocumentNumber = transfer.TransferNumber,
                    Quantity = item.Quantity,
                    PreviousQuantity = previousSourceStock,
                    NewQuantity = sourceStock.Quantity,
                    MovementDate = transfer.TransferDate,
                    Description = "Transfer " + transfer.TransferNumber + " - Çıkış",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                await _context.StockMovements.AddAsync(transferOut, cancellationToken);

                // TransferIn Movement
                var transferIn = new StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = transfer.DestinationWarehouseId,
                    MovementType = StockMovementType.TransferIn,
                    SourceType = StockMovementSourceType.StockTransfer,
                    DocumentNumber = transfer.TransferNumber,
                    Quantity = item.Quantity,
                    PreviousQuantity = previousDestStock,
                    NewQuantity = destStock.Quantity,
                    MovementDate = transfer.TransferDate,
                    Description = "Transfer " + transfer.TransferNumber + " - Giriş",
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                await _context.StockMovements.AddAsync(transferIn, cancellationToken);
            }

            transfer.Status = StockTransferStatus.Completed;
            transfer.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConcurrencyConflictException("Transfer işlemi sırasında eşzamanlı değişiklik saptandı.");
        }
        catch (InvalidOperationException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task CancelTransferAsync(int transferId, byte[] rowVersion, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var transfer = await _context.StockTransfers
                .FirstOrDefaultAsync(t => t.Id == transferId && !t.IsDeleted, cancellationToken);

            if (transfer == null)
            {
                throw new InvalidOperationException("Transfer bulunamadı.");
            }

            if (transfer.Status != StockTransferStatus.Draft)
            {
                throw new InvalidOperationException("Sadece taslak (Draft) statüsündeki transferler iptal edilebilir.");
            }

            _context.Entry(transfer).Property(t => t.RowVersion).OriginalValue = rowVersion;

            transfer.Status = StockTransferStatus.Cancelled;
            transfer.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConcurrencyConflictException("İşlem sırasında eşzamanlı değişiklik saptandı.");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
