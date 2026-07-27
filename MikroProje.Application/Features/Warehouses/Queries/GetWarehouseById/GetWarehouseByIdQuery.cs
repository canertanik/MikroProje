using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Features.Warehouses.DTOs;

namespace MikroProje.Application.Features.Warehouses.Queries.GetWarehouseById;

public class GetWarehouseByIdQuery : IRequest<Result<WarehouseDto>>
{
    public int Id { get; set; }
}
