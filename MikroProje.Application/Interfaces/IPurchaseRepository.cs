using MikroProje.Application.Common.Pagination;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Interfaces;

public interface IPurchaseRepository
{
    Task<Purchase?> GetByIdAsync(int id, CancellationToken cancellationToken);
    
    Task<(IReadOnlyCollection<Purchase> Items, int TotalCount)> GetAllAsync(
        int pageNumber, int pageSize, CancellationToken cancellationToken);
        
    Task<Purchase> CreatePurchaseAsync(
        Purchase purchase,
        List<PurchaseItem> lineItems,
        CurrentAccount currentAccount,
        CancellationToken cancellationToken);

    Task<List<MikroProje.Application.Features.SupplierStatements.DTOs.SupplierStatementItemDto>> GetStatementPurchasesAsync(
        int currentAccountId, CancellationToken cancellationToken);
}
