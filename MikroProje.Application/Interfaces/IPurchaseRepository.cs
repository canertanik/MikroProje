using MikroProje.Domain.Entities;

namespace MikroProje.Application.Interfaces;

public interface IPurchaseRepository
{
    Task<Purchase?> GetByIdAsync(int id, CancellationToken cancellationToken);
    
    Task<(IReadOnlyCollection<Purchase> Items, int TotalCount)> GetAllAsync(
        string? searchTerm, DateTime? startDate, DateTime? endDate, MikroProje.Domain.Enums.PurchaseStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken);
        
    Task<Purchase> CreatePurchaseAsync(
        Purchase purchase,
        List<PurchaseItem> lineItems,
        CancellationToken cancellationToken);

    Task<Purchase> ReceivePurchaseAsync(int purchaseId, CancellationToken cancellationToken);

    Task CancelPurchaseAsync(int purchaseId, CancellationToken cancellationToken);

    Task DeletePurchaseAsync(int purchaseId, CancellationToken cancellationToken);

    Task<List<MikroProje.Application.Features.SupplierStatements.DTOs.SupplierStatementItemDto>> GetStatementPurchasesAsync(
        int currentAccountId, CancellationToken cancellationToken);
}
