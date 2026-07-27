using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;
using MikroProje.Application.Features.StockTransfers.DTOs;

namespace MikroProje.Application.Features.StockTransfers.Queries.GetStockTransferById;

public class GetStockTransferByIdQueryHandler : IRequestHandler<GetStockTransferByIdQuery, Result<StockTransferDto>>
{
    private readonly IStockTransferRepository _repository;

    public GetStockTransferByIdQueryHandler(IStockTransferRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<StockTransferDto>> Handle(GetStockTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (dto == null)
        {
            return Result<StockTransferDto>.Fail("Transfer bulunamadý.", 404);
        }

        return Result<StockTransferDto>.Ok(dto);
    }
}
