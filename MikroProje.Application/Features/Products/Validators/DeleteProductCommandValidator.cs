using FluentValidation;
using MikroProje.Application.Features.Products.Commands.DeleteProduct;

namespace MikroProje.Application.Features.Products.Validators;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
