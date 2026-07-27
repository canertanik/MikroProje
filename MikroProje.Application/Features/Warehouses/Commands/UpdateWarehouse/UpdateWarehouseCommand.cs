using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Features.Warehouses.DTOs;

namespace MikroProje.Application.Features.Warehouses.Commands.UpdateWarehouse;

public class UpdateWarehouseCommand : IRequest<Result<WarehouseDto>>
{
    public int Id { get; set; }
    public UpdateWarehouseRequestDto Dto { get; set; } = null!;
}
