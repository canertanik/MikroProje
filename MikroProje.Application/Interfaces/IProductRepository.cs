using MikroProje.Domain.Entities;

namespace MikroProje.Application.Interfaces;

public interface IProductRepository
{
    Task<bool> CodeExistsAsync(string code, int? excludedId, CancellationToken cancellationToken);

    Task<bool> BarcodeExistsAsync(string? barcode, int? excludedId, CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<(IReadOnlyCollection<Product> Items, int TotalCount)> GetPagedAsync(string? search, bool? criticalOnly, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<(IReadOnlyCollection<Product> Items, int TotalCount)> GetCriticalStockPagedAsync(string? search, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<Product> CreateWithInitialStockAsync(Product product, int initialStockQuantity, CancellationToken cancellationToken);

    Task DeleteSoftAsync(Product product, CancellationToken cancellationToken);
}