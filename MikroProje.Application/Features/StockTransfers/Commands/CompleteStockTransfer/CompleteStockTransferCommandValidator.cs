using FluentValidation;

namespace MikroProje.Application.Features.StockTransfers.Commands.CompleteStockTransfer;

public class CompleteStockTransferCommandValidator : AbstractValidator<CompleteStockTransferCommand>
{
    public CompleteStockTransferCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Geçerli bir transfer ID'si gereklidir.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("RowVersion gereklidir.");
    }
}
