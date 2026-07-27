using MikroProje.Application.Common.Pagination;
using MikroProje.Domain.Entities;
using MikroProje.Application.Features.StockTransfers.DTOs;

namespace MikroProje.Application.Interfaces;

public interface IStockTransferRepository
{
    Task<string> GenerateTransferNumberAsync(CancellationToken cancellationToken);
    
    Task<StockTransferDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    
    Task<PagedResult<StockTransferListDto>> GetAllPagedAsync(string? search, int? sourceWarehouseId, int? destinationWarehouseId, MikroProje.Domain.Enums.StockTransferStatus? status, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, CancellationToken cancellationToken);
    
    Task AddAsync(StockTransfer stockTransfer, CancellationToken cancellationToken);
    
    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task CompleteTransferAsync(int transferId, byte[] rowVersion, CancellationToken cancellationToken);
    
    Task CancelTransferAsync(int transferId, byte[] rowVersion, CancellationToken cancellationToken);
}
