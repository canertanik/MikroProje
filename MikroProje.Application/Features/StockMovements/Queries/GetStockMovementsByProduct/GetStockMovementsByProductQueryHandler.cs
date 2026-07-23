using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.StockMovements.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.StockMovements.Queries.GetStockMovementsByProduct;

public class GetStockMovementsByProductQueryHandler : IRequestHandler<GetStockMovementsByProductQuery, Result<PagedResult<StockMovementDto>>>
{
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetStockMovementsByProductQueryHandler(IStockMovementRepository stockMovementRepository, IProductRepository productRepository, IMapper mapper)
    {
        _stockMovementRepository = stockMovementRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<StockMovementDto>>> Handle(GetStockMovementsByProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<PagedResult<StockMovementDto>>.Fail("Product not found.", 404);
        }

        var (items, totalCount) = await _stockMovementRepository.GetByProductAsync(
            request.ProductId,
            request.StartDate,
            request.EndDate,
            request.MovementType,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = _mapper.Map<IReadOnlyCollection<StockMovementDto>>(items);
        var pagedResult = PagedResult<StockMovementDto>.Create(dtos, request.PageNumber, request.PageSize, totalCount);

        return Result<PagedResult<StockMovementDto>>.Ok(pagedResult, "Stock movements listed successfully.");
    }
}
