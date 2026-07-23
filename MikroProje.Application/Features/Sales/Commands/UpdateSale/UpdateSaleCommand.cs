using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Sales.DTOs;

namespace MikroProje.Application.Features.Sales.Commands.UpdateSale;

public class UpdateSaleCommand : IRequest<Result<SaleDto>>
{
    public int Id { get; set; }

    public string? Description { get; set; }
}
