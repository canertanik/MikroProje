using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.SupplierStatements.DTOs;

namespace MikroProje.Application.Features.SupplierStatements.Queries.GetSupplierStatement;

public class GetSupplierStatementQuery : IRequest<Result<SupplierStatementResponseDto>>
{
    public int CurrentAccountId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
