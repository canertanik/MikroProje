using FluentValidation;

namespace MikroProje.Application.Features.StockTransfers.Commands.CancelStockTransfer;

public class CancelStockTransferCommandValidator : AbstractValidator<CancelStockTransferCommand>
{
    public CancelStockTransferCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçerli bir transfer ID'si gereklidir.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("RowVersion gereklidir.");
    }
}
