using MikroProje.Domain.Entities;

namespace MikroProje.Application.Interfaces;

public interface ISupplierPaymentRepository
{
    Task<SupplierPayment?> GetByIdAsync(int id, CancellationToken cancellationToken);
    
    Task<(IReadOnlyCollection<SupplierPayment> Items, int TotalCount)> GetAllAsync(int? currentAccountId, int pageNumber, int pageSize, CancellationToken cancellationToken);
    
    Task<SupplierPayment> CreateAsync(SupplierPayment supplierPayment, CurrentAccount currentAccount, CancellationToken cancellationToken);
    
    Task<List<MikroProje.Application.Features.SupplierStatements.DTOs.SupplierStatementItemDto>> GetStatementPaymentsAsync(
        int currentAccountId, CancellationToken cancellationToken);
}
