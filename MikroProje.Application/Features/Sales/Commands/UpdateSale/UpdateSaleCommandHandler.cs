using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Caching;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Sales.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Sales.Commands.UpdateSale;

public class UpdateSaleCommandHandler : IRequestHandler<UpdateSaleCommand, Result<SaleDto>>
{
    private readonly ICacheService _cacheService;
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public UpdateSaleCommandHandler(ISaleRepository saleRepository, IMapper mapper, ICacheService cacheService)
    {
        _cacheService = cacheService;
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<Result<SaleDto>> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
        {
            return Result<SaleDto>.Fail($"Satış (Id={request.Id}) bulunamadı veya iptal edilmiş.", 404);
        }

        sale.Description = request.Description;
        sale.UpdatedDate = DateTime.UtcNow;

        try
        {
            var updated = await _saleRepository.UpdateSaleAsync(sale, cancellationToken);
            var dto = _mapper.Map<SaleDto>(updated);
            return Result<SaleDto>.Ok(dto, "Satış başarıyla güncellendi.");
        }
        catch (ConcurrencyConflictException)
        {
            return Result<SaleDto>.Fail("Eş zamanlı güncelleme tespit edildi. Lütfen tekrar deneyin.", 409);
        }
    }
}
