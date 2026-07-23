using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Sales.DTOs;

namespace MikroProje.Application.Features.Sales.Queries.GetSaleById;

public class GetSaleByIdQuery : IRequest<Result<SaleDto>>
{
    public int Id { get; set; }
}
