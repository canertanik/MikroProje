using MikroProje.Domain.Entities;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Interfaces;

public interface IStockMovementRepository
{
    Task<(IReadOnlyCollection<StockMovement> Items, int TotalCount)> GetByProductAsync(int productId, DateTime? startDate, DateTime? endDate, StockMovementType? movementType, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<StockMovement> CreateAsync(int productId, StockMovementType movementType, StockMovementSourceType sourceType, int quantity, string? documentNumber, string? description, DateTime movementDate, CancellationToken cancellationToken);
}