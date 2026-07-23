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
}