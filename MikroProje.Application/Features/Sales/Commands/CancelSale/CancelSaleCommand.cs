using MediatR;
using MikroProje.Application.Common.Results;

namespace MikroProje.Application.Features.Sales.Commands.CancelSale;

public class CancelSaleCommand : IRequest<Result<bool>>
{
    public int Id { get; set; }
}
