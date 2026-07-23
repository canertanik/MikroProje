using FluentValidation;
using MikroProje.Application.Features.StockMovements.Commands.CreateStockMovement;

namespace MikroProje.Application.Features.StockMovements.Validators;

public class CreateStockMovementCommandValidator : AbstractValidator<CreateStockMovementCommand>
{
    public CreateStockMovementCommandValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.DocumentNumber).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
