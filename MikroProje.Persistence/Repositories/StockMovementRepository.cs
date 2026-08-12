using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;
using MikroProje.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MikroProje.Persistence.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly MikroProjeDbContext _dbContext;

    public StockMovementRepository(MikroProjeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyCollection<StockMovement> Items, int TotalCount)> GetByProductAsync(int productId, DateTime? startDate, DateTime? endDate, StockMovementType? movementType, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.StockMovements
            .AsNoTracking()
            .Include(x => x.Product)
            .Where(x => x.ProductId == productId);

        if (startDate.HasValue)
        {
            query = query.Where(x => x.MovementDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(x => x.MovementDate <= endDate.Value);
        }

        if (movementType.HasValue)
        {
            query = query.Where(x => x.MovementType == movementType.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.MovementDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<StockMovement> CreateAsync(int productId, StockMovementType movementType, StockMovementSourceType sourceType, int quantity, string? documentNumber, string? description, DateTime movementDate, CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(x => x.Id == productId && !x.IsDeleted, cancellationToken);

            if (product is null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            var previousQuantity = product.StockQuantity;
            var increasing = movementType is StockMovementType.StockIn or StockMovementType.AdjustmentIncrease;
            var newQuantity = increasing ? previousQuantity + quantity : previousQuantity - quantity;

            if (!increasing && previousQuantity < quantity)
            {
                throw new InvalidOperationException($"Insufficient stock. Current stock: {previousQuantity}, requested quantity: {quantity}.");
            }

            var movement = new StockMovement
            {
                ProductId = productId,
                MovementType = movementType,
                SourceType = sourceType,
                Quantity = quantity,
                PreviousQuantity = previousQuantity,
                NewQuantity = newQuantity,
                DocumentNumber = documentNumber,
                Description = description,
                MovementDate = movementDate
            };

            product.StockQuantity = newQuantity;
            product.UpdatedDate = DateTime.UtcNow;

            await _dbContext.StockMovements.AddAsync(movement, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            movement.Product = product;

            return movement;
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
        });
    }
}
