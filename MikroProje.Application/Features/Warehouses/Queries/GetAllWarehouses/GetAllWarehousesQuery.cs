using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Features.Warehouses.DTOs;

namespace MikroProje.Application.Features.Warehouses.Queries.GetAllWarehouses;

public class GetAllWarehousesQuery : IRequest<Result<PagedResult<WarehouseListDto>>>
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDefault { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
