using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.SupplierStatements.DTOs;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Enums;

namespace MikroProje.Application.Features.SupplierStatements.Queries.GetSupplierStatement;

public class GetSupplierStatementQueryHandler : IRequestHandler<GetSupplierStatementQuery, Result<SupplierStatementResponseDto>>
{
    private readonly ICurrentAccountRepository _currentAccountRepository;
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ISupplierPaymentRepository _supplierPaymentRepository;

    public GetSupplierStatementQueryHandler(
        ICurrentAccountRepository currentAccountRepository,
        IPurchaseRepository purchaseRepository,
        ISupplierPaymentRepository supplierPaymentRepository)
    {
        _currentAccountRepository = currentAccountRepository;
        _purchaseRepository = purchaseRepository;
        _supplierPaymentRepository = supplierPaymentRepository;
    }

    public async Task<Result<SupplierStatementResponseDto>> Handle(GetSupplierStatementQuery request, CancellationToken cancellationToken)
    {
        var currentAccount = await _currentAccountRepository.GetByIdAsync(request.CurrentAccountId, cancellationToken);
        if (currentAccount == null)
        {
            return Result<SupplierStatementResponseDto>.Fail("Current account not found.", 404);
        }

        if (currentAccount.Type != CurrentAccountType.Supplier && currentAccount.Type != CurrentAccountType.Both)
        {
            return Result<SupplierStatementResponseDto>.Fail("Only supplier current accounts can have supplier statements.", 400);
        }

        var purchases = await _purchaseRepository.GetStatementPurchasesAsync(request.CurrentAccountId, cancellationToken);
        var payments = await _supplierPaymentRepository.GetStatementPaymentsAsync(request.CurrentAccountId, cancellationToken);

        var allTransactions = purchases.Concat(payments)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.DocumentType)
            .ThenBy(x => x.DocumentId)
            .ToList();

        decimal runningBalance = 0;
        foreach (var txn in allTransactions)
        {
            runningBalance += txn.Debit - txn.Credit;
            txn.RunningBalance = Math.Round(runningBalance, 2);
        }

        var query = allTransactions.AsEnumerable();

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

        var pagedItems = filteredList
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var pagedResult = PagedResult<SupplierStatementItemDto>.Create(
            pagedItems,
            request.PageNumber,
            request.PageSize,
            totalCount);

        var response = new SupplierStatementResponseDto
        {
            CurrentAccountId = currentAccount.Id,
            CurrentAccountName = currentAccount.Name,
            Items = pagedResult
        };

        return Result<SupplierStatementResponseDto>.Ok(response);
    }
}
