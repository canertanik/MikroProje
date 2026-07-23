using FluentValidation;
using MikroProje.Application.Features.Sales.Commands.UpdateSale;

namespace MikroProje.Application.Features.Sales.Validators;

public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id sıfırdan büyük olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}
