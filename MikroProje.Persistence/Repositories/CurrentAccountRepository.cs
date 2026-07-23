using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace MikroProje.Persistence.Repositories;

public class CurrentAccountRepository : ICurrentAccountRepository
{
    private readonly MikroProjeDbContext _dbContext;

    public CurrentAccountRepository(MikroProjeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludedId, CancellationToken cancellationToken)
    {
        return await _dbContext.CurrentAccounts
            .AsNoTracking()
            .AnyAsync(x => x.Code == code && (!excludedId.HasValue || x.Id != excludedId.Value), cancellationToken);
    }

    public async Task<CurrentAccount?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.CurrentAccounts
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyCollection<CurrentAccount>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.CurrentAccounts
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(CurrentAccount currentAccount, CancellationToken cancellationToken)
    {
        await _dbContext.CurrentAccounts.AddAsync(currentAccount, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.CurrentAccounts
            .AsNoTracking()
            .AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    public async Task<List<MikroProje.Application.Features.CurrentAccounts.DTOs.StatementDto>> GetStatementTransactionsAsync(int currentAccountId, CancellationToken cancellationToken)
    {
        var sales = await _dbContext.Sales
            .AsNoTracking()
            .Where(s => s.CurrentAccountId == currentAccountId && !s.IsDeleted)
            .Select(s => new MikroProje.Application.Features.CurrentAccounts.DTOs.StatementDto
            {
                Date = s.SaleDate,
                DocumentType = MikroProje.Domain.Enums.DocumentType.Sale,
                DocumentId = s.Id,
                Description = s.Description ?? string.Empty,
                Debit = s.GrandTotal,
                Credit = 0,
                BalanceAfterTransaction = 0
            })
            .ToListAsync(cancellationToken);

        var payments = await _dbContext.Payments
            .AsNoTracking()
            .Where(p => p.CurrentAccountId == currentAccountId && !p.IsDeleted)
            .Select(p => new MikroProje.Application.Features.CurrentAccounts.DTOs.StatementDto
            {
                Date = p.PaymentDate,
                DocumentType = MikroProje.Domain.Enums.DocumentType.Payment,
                DocumentId = p.Id,
                Description = p.Description ?? string.Empty,
                Debit = 0,
                Credit = p.Amount,
                BalanceAfterTransaction = 0
            })
            .ToListAsync(cancellationToken);

        return sales.Concat(payments)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.DocumentType)
            .ThenBy(x => x.DocumentId)
            .ToList();
    }
}