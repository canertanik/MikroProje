using FluentValidation;
using MikroProje.Application.Features.Products.Commands.CreateProduct;

namespace MikroProje.Application.Features.Products.Validators;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Barcode).MaximumLength(100);
        RuleFor(x => x.PurchasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SalePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.VatRate).InclusiveBetween(0, 100);
        RuleFor(x => x.CriticalStockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.InitialStockQuantity).GreaterThanOrEqualTo(0);
    }
}
