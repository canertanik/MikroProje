using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Purchases.Commands.CancelPurchase;

public class CancelPurchaseCommandHandler : IRequestHandler<CancelPurchaseCommand, Result<bool>>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ICacheService _cacheService;

    public CancelPurchaseCommandHandler(
        IPurchaseRepository purchaseRepository,
        ICacheService cacheService)
    {
        _purchaseRepository = purchaseRepository;
        _cacheService = cacheService;
    }

    public async Task<Result<bool>> Handle(CancelPurchaseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _purchaseRepository.CancelPurchaseAsync(request.Id, cancellationToken);

            // Dashboard cache invalidation (since pending items might be displayed on dashboard, or total count changed)
            await _cacheService.RemoveByPrefixAsync(CacheKeys.DashboardPrefix, cancellationToken);

            return Result<bool>.Ok(true, "Satın alma kaydı başarıyla iptal edildi.");
        }
        catch (KeyNotFoundException ex)
        {
            return Result<bool>.Fail(ex.Message, 404);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Fail(ex.Message, 400); // 400 Bad Request or 409 Conflict, using 400 for business rule violation
        }
    }
}
