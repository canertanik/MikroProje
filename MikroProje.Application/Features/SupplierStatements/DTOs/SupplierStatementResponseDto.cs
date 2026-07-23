using MikroProje.Application.Common.Pagination;

namespace MikroProje.Application.Features.SupplierStatements.DTOs;

public class SupplierStatementResponseDto
{
    public int CurrentAccountId { get; set; }
    
    public string CurrentAccountName { get; set; } = string.Empty;
    
    public PagedResult<SupplierStatementItemDto> Items { get; set; } = null!;
}
