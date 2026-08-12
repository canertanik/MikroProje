using Microsoft.EntityFrameworkCore;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Persistence.Contexts;

namespace MikroProje.Persistence.Repositories;

public class SupplierPaymentRepository : ISupplierPaymentRepository
{
    private readonly MikroProjeDbContext _dbContext;

    public SupplierPaymentRepository(MikroProjeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SupplierPayment?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.SupplierPayments
            .AsNoTracking()
            .Select(sp => new SupplierPayment
            {
                Id = sp.Id,
                CurrentAccountId = sp.CurrentAccountId,
                PaymentDate = sp.PaymentDate,
                Amount = sp.Amount,
                PaymentMethod = sp.PaymentMethod,
                ReferenceNumber = sp.ReferenceNumber,
                Description = sp.Description,
                CreatedDate = sp.CreatedDate,
                CurrentAccount = new CurrentAccount
                {
                    Id = sp.CurrentAccount.Id,
                    Name = sp.CurrentAccount.Name
                }
            })
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<SupplierPayment> Items, int TotalCount)> GetAllAsync(
        int? currentAccountId, string? searchTerm, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _dbContext.SupplierPayments
            .AsNoTracking()
            .Include(sp => sp.CurrentAccount)
            .Where(sp => !sp.IsDeleted);

        if (currentAccountId.HasValue)
        {
            query = query.Where(sp => sp.CurrentAccountId == currentAccountId.Value);
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

            query = query.Where(sp => (isNumericId && sp.Id == searchId) || 
                                      sp.CurrentAccount.Name.ToLower().Contains(term) ||
                                      (sp.Description != null && sp.Description.ToLower().Contains(term)) ||
                                      (sp.ReferenceNumber != null && sp.ReferenceNumber.ToLower().Contains(term)));
        }

        if (startDate.HasValue)
        {
            query = query.Where(sp => sp.PaymentDate >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(sp => sp.PaymentDate <= end);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .OrderByDescending(sp => sp.PaymentDate)
            .ThenByDescending(sp => sp.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(sp => new SupplierPayment
            {
                Id = sp.Id,
                CurrentAccountId = sp.CurrentAccountId,
                PaymentDate = sp.PaymentDate,
                Amount = sp.Amount,
                PaymentMethod = sp.PaymentMethod,
                ReferenceNumber = sp.ReferenceNumber,
                Description = sp.Description,
                CreatedDate = sp.CreatedDate,
                CurrentAccount = new CurrentAccount
                {
                    Id = sp.CurrentAccount.Id,
                    Name = sp.CurrentAccount.Name
                }
            })
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<SupplierPayment> CreateAsync(SupplierPayment supplierPayment, CurrentAccount currentAccount, CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
            await _dbContext.SupplierPayments.AddAsync(supplierPayment, cancellationToken);
            
            // Tedarikçiye ödeme yapılınca bizim borcumuz Amount kadar azalır. (Net bakiye pozitif yönde artar)
            currentAccount.Balance += supplierPayment.Amount;
            currentAccount.UpdatedDate = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            supplierPayment.CurrentAccount = currentAccount;
            return supplierPayment;
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

    public async Task<List<MikroProje.Application.Features.SupplierStatements.DTOs.SupplierStatementItemDto>> GetStatementPaymentsAsync(
        int currentAccountId, CancellationToken cancellationToken)
    {
        return await _dbContext.SupplierPayments
            .AsNoTracking()
            .Where(x => x.CurrentAccountId == currentAccountId && !x.IsDeleted)
            .Select(x => new MikroProje.Application.Features.SupplierStatements.DTOs.SupplierStatementItemDto
            {
                Date = x.PaymentDate,
                DocumentType = MikroProje.Domain.Enums.DocumentType.SupplierPayment,
                DocumentId = x.Id,
                Description = x.Description ?? string.Empty,
                Debit = 0,
                Credit = x.Amount,
                RunningBalance = 0
            })
            .ToListAsync(cancellationToken);
    }
}
