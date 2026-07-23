using FluentValidation;
using MikroProje.Application.Features.Sales.Commands.CreateSale;

namespace MikroProje.Application.Features.Sales.Validators;

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.CurrentAccountId)
            .GreaterThan(0)
            .WithMessage("CurrentAccountId sıfırdan büyük olmalıdır.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("En az 1 ürün kalemi girilmelidir.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("ProductId sıfırdan büyük olmalıdır.");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Miktar sıfırdan büyük olmalıdır.");

            item.RuleFor(x => x.Discount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("İndirim negatif olamaz.")
                .LessThanOrEqualTo(100)
                .WithMessage("İndirim %100'ü geçemez.");

            item.When(x => x.UnitPrice.HasValue, () =>
            {
                item.RuleFor(x => x.UnitPrice!.Value)
                    .GreaterThan(0)
                    .WithMessage("Birim fiyat sıfırdan büyük olmalıdır.");
            });
        });

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}
