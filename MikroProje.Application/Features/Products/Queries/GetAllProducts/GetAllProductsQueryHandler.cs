using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Products.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<PagedResult<ProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetAllProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _productRepository.GetPagedAsync(request.Search, request.CriticalOnly, request.PageNumber, request.PageSize, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyCollection<ProductDto>>(items);
        var pagedResult = PagedResult<ProductDto>.Create(dtos, request.PageNumber, request.PageSize, totalCount);

        return Result<PagedResult<ProductDto>>.Ok(pagedResult, "Products listed successfully.");
    }
}
