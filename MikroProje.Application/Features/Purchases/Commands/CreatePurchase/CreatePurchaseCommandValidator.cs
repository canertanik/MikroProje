using FluentValidation;

namespace MikroProje.Application.Features.Purchases.Commands.CreatePurchase;

public class CreatePurchaseCommandValidator : AbstractValidator<CreatePurchaseCommand>
{
    public CreatePurchaseCommandValidator()
    {
        RuleFor(x => x.CurrentAccountId)
            .GreaterThan(0).WithMessage("Geçerli bir cari hesap seçilmelidir.");

        RuleFor(x => x.WarehouseId)
            .GreaterThan(0).WithMessage("Geçerli bir depo seçilmelidir.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Satın alma işleminde en az bir kalem bulunmalıdır.");

        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("Geçerli bir ürün seçilmelidir.");

            items.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Miktar sıfırdan büyük olmalıdır.");

            items.RuleFor(i => i.UnitPrice)
                .GreaterThan(0).When(i => i.UnitPrice.HasValue)
                .WithMessage("Birim fiyatı sıfırdan büyük olmalıdır.");
        });

        // Fail early if there are duplicate products to keep logic simple and safe
        RuleFor(x => x.Items)
            .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .When(x => x.Items != null && x.Items.Any())
            .WithMessage("Aynı ürün faturada birden fazla kez bulunamaz. Lütfen miktarları birleştirin.");
    }
}
