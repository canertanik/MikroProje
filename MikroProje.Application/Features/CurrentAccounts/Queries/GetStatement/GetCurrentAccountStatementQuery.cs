using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.CurrentAccounts.DTOs;

namespace MikroProje.Application.Features.CurrentAccounts.Queries.GetStatement;

public class GetCurrentAccountStatementQuery : IRequest<Result<PagedResult<StatementDto>>>
{
    public int CurrentAccountId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
