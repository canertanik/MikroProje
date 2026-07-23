using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Products.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Products.Queries.GetCriticalStockProducts;

public class GetCriticalStockProductsQueryHandler : IRequestHandler<GetCriticalStockProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetCriticalStockProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetCriticalStockProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _productRepository.GetCriticalStockPagedAsync(request.Search, request.PageNumber, request.PageSize, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyCollection<ProductDto>>(items);
        var pagedResult = PagedResult<ProductDto>.Create(dtos, request.PageNumber, request.PageSize, totalCount);

        return Result<PagedResult<ProductDto>>.Ok(pagedResult, "Critical stock products listed successfully.");
    }
}
