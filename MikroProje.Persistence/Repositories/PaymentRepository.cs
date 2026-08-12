using Microsoft.EntityFrameworkCore;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Persistence.Contexts;

namespace MikroProje.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly MikroProjeDbContext _dbContext;

    public PaymentRepository(MikroProjeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Payments
            .Include(p => p.CurrentAccount)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Payment> Items, int TotalCount)> GetAllAsync(
        int? currentAccountId, string? searchTerm, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.Payments
            .Include(p => p.CurrentAccount)
            .Where(p => !p.IsDeleted);

        if (currentAccountId.HasValue)
        {
            query = query.Where(p => p.CurrentAccountId == currentAccountId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower().Trim();
            
            var termForId = term.Replace("#", "").Trim();
            if (termForId.StartsWith("0"))
                termForId = termForId.TrimStart('0');
            
            if (string.IsNullOrEmpty(termForId))
                termForId = term;

            bool isNumericId = int.TryParse(termForId, out int searchId);

            query = query.Where(p => (isNumericId && p.Id == searchId) || 
                                     p.CurrentAccount.Name.ToLower().Contains(term) ||
                                     (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        if (startDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(p => p.PaymentDate <= end);
        }

        query = query.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.CreatedDate);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Payment> CreateAsync(Payment payment, CurrentAccount currentAccount, CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
            await _dbContext.Payments.AddAsync(payment, cancellationToken);
            
            // Tahsilat/Ödeme oluşturulunca CurrentAccount.Balance alanından Amount kadar düşülür.
            currentAccount.Balance -= payment.Amount;
            currentAccount.UpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            payment.CurrentAccount = currentAccount;
                return payment;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new ConcurrencyConflictException(ex.Message);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public async Task<Payment> UpdateAsync(Payment payment, byte[] originalRowVersion, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Entry(payment).Property(p => p.RowVersion).OriginalValue = originalRowVersion;
            payment.UpdatedDate = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
            return payment;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(ex.Message);
        }
    }

    public async Task DeleteAsync(Payment payment, CancellationToken cancellationToken)
    {
        if (!_dbContext.Entry(payment).Reference(x => x.CurrentAccount).IsLoaded)
        {
            await _dbContext.Entry(payment).Reference(x => x.CurrentAccount).LoadAsync(cancellationToken);
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
            // Soft delete
            payment.IsDeleted = true;
            payment.UpdatedDate = DateTime.UtcNow;

            // Bakiye iadesi
            payment.CurrentAccount.Balance += payment.Amount;
            payment.CurrentAccount.UpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ConcurrencyConflictException(ex.Message);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
            }
        });
    }
}
