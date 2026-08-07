using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Purchases.Commands.DeletePurchase;

public class DeletePurchaseCommandHandler : IRequestHandler<DeletePurchaseCommand, Result<bool>>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ICacheService _cacheService;

    public DeletePurchaseCommandHandler(
        IPurchaseRepository purchaseRepository,
        ICacheService cacheService)
    {
        _purchaseRepository = purchaseRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(DeletePurchaseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _purchaseRepository.DeletePurchaseAsync(request.Id, cancellationToken);

            await _cacheService.RemoveByPrefixAsync(CacheKeys.DashboardPrefix, cancellationToken);

            return Result<bool>.Ok(true, "Satın alma kaydı başarıyla silindi.");
        }
        catch (KeyNotFoundException ex)
        {
            return Result<bool>.Fail(ex.Message, 404);
        }
    }
}
