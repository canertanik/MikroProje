using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Purchases.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Purchases.Commands.ReceivePurchase;

public class ReceivePurchaseCommandHandler : IRequestHandler<ReceivePurchaseCommand, Result<PurchaseDto>>
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public ReceivePurchaseCommandHandler(
        IPurchaseRepository purchaseRepository,
        ICacheService cacheService,
        IMapper mapper)
    {
        _purchaseRepository = purchaseRepository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<Result<PurchaseDto>> Handle(ReceivePurchaseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var purchase = await _purchaseRepository.ReceivePurchaseAsync(request.Id, cancellationToken);

            // Cache invalidation — stok, ürün, dashboard ve cari hesap verileri değişti
            await _cacheService.RemoveByPrefixAsync(CacheKeys.DashboardPrefix, cancellationToken);
            await _cacheService.RemoveByPrefixAsync(CacheKeys.ProductsPrefix, cancellationToken);
            await _cacheService.RemoveByPrefixAsync(CacheKeys.CurrentAccountsPrefix, cancellationToken);

            var dto = _mapper.Map<PurchaseDto>(purchase);
            return Result<PurchaseDto>.Ok(dto, "Satın alma depoya başarıyla alındı. Stoklar ve tedarikçi bakiyesi güncellendi.");
        }
        catch (KeyNotFoundException ex)
        {
            return Result<PurchaseDto>.Fail(ex.Message, 404);
        }
        catch (InvalidOperationException ex)
        {
            return Result<PurchaseDto>.Fail(ex.Message, 409);
        }
        catch (ConcurrencyConflictException)
        {
            return Result<PurchaseDto>.Fail("Eş zamanlı güncelleme tespit edildi. Lütfen tekrar deneyin.", 409);
        }
    }
}
