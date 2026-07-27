using MediatR;
using MikroProje.Application.Common.Exceptions;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.StockTransfers.Commands.CancelStockTransfer;

public class CancelStockTransferCommandHandler : IRequestHandler<CancelStockTransferCommand, Result<bool>>
{
    private readonly IStockTransferRepository _repository;

    public CancelStockTransferCommandHandler(IStockTransferRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(CancelStockTransferCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _repository.CancelTransferAsync(request.Id, request.RowVersion, cancellationToken);
            return Result<bool>.Ok(true, "Transfer baþarýyla iptal edildi.");
        }
        catch (ConcurrencyConflictException ex)
        {
            return Result<bool>.Fail(ex.Message, 409);
        }
        catch (InvalidOperationException ex)
        {
            return Result<bool>.Fail(ex.Message, 400);
        }
    }
}
