using FluentValidation;
using MikroProje.Application.Features.Sales.Commands.CancelSale;

namespace MikroProje.Application.Features.Sales.Validators;

public class CancelSaleCommandValidator : AbstractValidator<CancelSaleCommand>
{
    public CancelSaleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id sıfırdan büyük olmalıdır.");
    }
}
