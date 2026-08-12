using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly ICacheService _cacheService;
    private readonly IProductRepository _productRepository;

    public DeleteProductCommandHandler(IProductRepository productRepository, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _productRepository = productRepository;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return Result<bool>.Fail("Product not found.", 404);
        }

        try
        {
            await _productRepository.DeleteSoftAsync(product, cancellationToken);
            await _cacheService.RemoveByPrefixAsync(CacheKeys.DashboardPrefix, cancellationToken);
            return Result<bool>.NoContent();
        }
        catch (ConcurrencyConflictException)
        {
            return Result<bool>.Fail("Concurrent update detected.", 409);
        }
    }
}
