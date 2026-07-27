using MikroProje.Domain.Entities;
using MikroProje.Application.Common.Pagination;

namespace MikroProje.Application.Interfaces;

public interface IWarehouseRepository
{
    Task<bool> CodeExistsAsync(string code, int? excludedId, CancellationToken cancellationToken);
    Task<Warehouse?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Warehouse?> GetDefaultWarehouseAsync(CancellationToken cancellationToken);
    Task<PagedResult<Warehouse>> GetAllPagedAsync(string? search, bool? isActive, bool? isDefault, int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<bool> HasRelatedRecordsAsync(int id, CancellationToken cancellationToken);
    Task<Warehouse> UpdateAsync(Warehouse warehouse, byte[] originalRowVersion, CancellationToken cancellationToken);
}
