using MikroProje.Domain.Entities;

namespace MikroProje.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<(IReadOnlyCollection<Payment> Items, int TotalCount)> GetAllAsync(
        int? currentAccountId, string? searchTerm, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    /// Payment INSERT + CurrentAccount.Balance -= Amount — tek transaction.
    /// </summary>
    Task<Payment> CreateAsync(Payment payment, CurrentAccount currentAccount, CancellationToken cancellationToken);

    /// <summary>
    /// Yalnızca header alanlarını (Description, PaymentMethod vb.) günceller.
    /// Bakiye değişmez. Concurrency için rowVersion gönderilir.
    /// </summary>
    Task<Payment> UpdateAsync(Payment payment, byte[] originalRowVersion, CancellationToken cancellationToken);

    /// <summary>
    /// Soft delete + CurrentAccount.Balance += Amount — tek transaction.
    /// </summary>
    Task DeleteAsync(Payment payment, CancellationToken cancellationToken);
}
