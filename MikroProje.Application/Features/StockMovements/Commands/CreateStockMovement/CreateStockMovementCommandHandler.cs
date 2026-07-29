using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.StockMovements.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.StockMovements.Commands.CreateStockMovement;

public class CreateStockMovementCommandHandler : IRequestHandler<CreateStockMovementCommand, Result<StockMovementDto>>
{
    private readonly ICacheService _cacheService;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CreateStockMovementCommandHandler(IStockMovementRepository stockMovementRepository, IProductRepository productRepository, IMapper mapper, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _stockMovementRepository = stockMovementRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<Result<StockMovementDto>> Handle(CreateStockMovementCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result<StockMovementDto>.Fail("Product not found.", 404);
        }

        try
        {
            var movementDate = request.MovementDate ?? DateTime.UtcNow;
            var stockMovement = await _stockMovementRepository.CreateAsync(
                request.ProductId,
                request.MovementType,
                request.SourceType,
                request.Quantity,
                request.DocumentNumber,
                request.Description,
                movementDate,
                cancellationToken);

            var dto = _mapper.Map<StockMovementDto>(stockMovement);
            return Result<StockMovementDto>.Created(dto, "Stock movement created successfully.");
        }
        catch (KeyNotFoundException)
        {
            return Result<StockMovementDto>.Fail("Product not found.", 404);
        }
        catch (InvalidOperationException exception)
        {
            return Result<StockMovementDto>.Fail(exception.Message, 400);
        }
        catch (ConcurrencyConflictException)
        {
            return Result<StockMovementDto>.Fail("Concurrent update detected.", 409);
        }
    }
}
