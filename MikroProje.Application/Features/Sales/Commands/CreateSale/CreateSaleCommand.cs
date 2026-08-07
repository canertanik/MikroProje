using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Sales.DTOs;

namespace MikroProje.Application.Features.Sales.Commands.CreateSale;

public class CreateSaleCommand : IRequest<Result<SaleDto>>
{
    public int CurrentAccountId { get; set; }

    public int WarehouseId { get; set; }

    public List<SaleItemDto> Items { get; set; } = new();

    public DateTime? SaleDate { get; set; }

    public string? Description { get; set; }
}
