using MikroProje.Domain.Entities;

namespace MikroProje.Application.Interfaces;

public interface ICurrentAccountRepository
{
    Task<bool> CodeExistsAsync(string code, int? excludedId, CancellationToken cancellationToken);

    Task<CurrentAccount?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CurrentAccount>> GetAllAsync(CancellationToken cancellationToken);

    Task AddAsync(CurrentAccount currentAccount, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    Task<List<MikroProje.Application.Features.CurrentAccounts.DTOs.StatementDto>> GetStatementTransactionsAsync(int currentAccountId, CancellationToken cancellationToken);
}