using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Products.DTOs;

namespace MikroProje.Application.Features.Products.Queries.GetProductStocks;

public class GetProductStocksQuery : IRequest<Result<List<ProductStockDto>>>
{
    public int ProductId { get; set; }
}
