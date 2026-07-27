using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Common.Pagination;

namespace MikroProje.Application.Features.Warehouses.Commands.DeleteWarehouse;

public class DeleteWarehouseCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
