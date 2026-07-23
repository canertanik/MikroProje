using MediatR;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Sales.Commands.CancelSale;

public class CancelSaleCommandHandler : IRequestHandler<CancelSaleCommand, Result<bool>>
{
    private readonly ISaleRepository _saleRepository;

    public CancelSaleCommandHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<Result<bool>> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        // IsDeleted dahil tüm satışları ara — zaten iptal edilmişi tekrar iptal etmeyelim
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (sale is null)
        {
            return Result<bool>.Fail($"Satış (Id={request.Id}) bulunamadı.", 404);
        }

        if (sale.IsDeleted)
        {
            return Result<bool>.Fail("Bu satış zaten iptal edilmiş.", 409);
        }

        try
        {
            await _saleRepository.CancelSaleAsync(sale, cancellationToken);
            return Result<bool>.NoContent("Satış başarıyla iptal edildi.");
        }
        catch (ConcurrencyConflictException)
        {
            return Result<bool>.Fail("Eş zamanlı güncelleme tespit edildi. Lütfen tekrar deneyin.", 409);
        }
    }
}
