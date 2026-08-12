using MikroProje.Application.Common.Pagination;

namespace MikroProje.Application.Features.CurrentAccounts.DTOs;

public class CurrentAccountStatementResponseDto
{
    public decimal CustomerBalance { get; set; }
    public PagedResult<StatementDto> Items { get; set; } = null!;
}
