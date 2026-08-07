using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.CurrentAccounts.Queries.GetStatement;

public class GetCurrentAccountStatementQueryHandler : IRequestHandler<GetCurrentAccountStatementQuery, Result<PagedResult<StatementDto>>>
{
    private readonly ICurrentAccountRepository _currentAccountRepository;

    public GetCurrentAccountStatementQueryHandler(ICurrentAccountRepository currentAccountRepository)
    {
        _currentAccountRepository = currentAccountRepository;
    }

    public async Task<Result<PagedResult<StatementDto>>> Handle(GetCurrentAccountStatementQuery request, CancellationToken cancellationToken)
    {
        var currentAccount = await _currentAccountRepository.GetByIdAsync(request.CurrentAccountId, cancellationToken);
        if (currentAccount is null)
        {
            return Result<PagedResult<StatementDto>>.Fail("Current account not found.", 404);
        }

        if (currentAccount.Type != MikroProje.Domain.Enums.CurrentAccountType.Customer && currentAccount.Type != MikroProje.Domain.Enums.CurrentAccountType.Both)
        {
            return Result<PagedResult<StatementDto>>.Fail("Cari ekstre yalnızca müşteri (Customer) veya Both türündeki cariler için alınabilir.", 400);
        }

        var allTransactions = await _currentAccountRepository.GetStatementTransactionsAsync(request.CurrentAccountId, cancellationToken);

        // Running Balance calculation from the beginning
        decimal runningBalance = 0;
        var calculatedTransactions = new List<StatementDto>();

        foreach (var txn in allTransactions)
        {
            runningBalance += txn.Debit - txn.Credit;
            
            calculatedTransactions.Add(new StatementDto
            {
                Date = txn.Date,
                DocumentType = txn.DocumentType,
                DocumentId = txn.DocumentId,
                Description = txn.Description,
                Debit = txn.Debit,
                Credit = txn.Credit,
                BalanceAfterTransaction = Math.Round(runningBalance, 2)
            });
        }

        // Apply filters
        var query = calculatedTransactions.AsEnumerable();

        if (request.StartDate.HasValue)
        {
            var startDate = request.StartDate.Value;

            if (startDate.Kind == DateTimeKind.Local)
                startDate = startDate.ToUniversalTime();
            else if (startDate.Kind == DateTimeKind.Unspecified)
                startDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);

            query = query.Where(x => x.Date >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = request.EndDate.Value;

            if (endDate.Kind == DateTimeKind.Local)
                endDate = endDate.ToUniversalTime();
            else if (endDate.Kind == DateTimeKind.Unspecified)
                endDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

            if (endDate.TimeOfDay == TimeSpan.Zero)
            {
                var endExclusive = endDate.Date.AddDays(1);
                query = query.Where(x => x.Date < endExclusive);
            }
            else
            {
                query = query.Where(x => x.Date <= endDate);
            }
        }

        var filteredList = query.ToList();
        var totalCount = filteredList.Count;

        // Apply pagination
        var pagedItems = filteredList
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var pagedResult = PagedResult<StatementDto>.Create(
            pagedItems,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result<PagedResult<StatementDto>>.Ok(pagedResult);
    }
}
