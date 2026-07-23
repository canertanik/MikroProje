using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Products.DTOs;

namespace MikroProje.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQuery : IRequest<Result<PagedResult<ProductDto>>>
{
    public string? Search { get; set; }

    public bool? CriticalOnly { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
