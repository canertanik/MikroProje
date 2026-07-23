using FluentValidation;
using MikroProje.Application.Features.Products.Queries.GetProductById;

namespace MikroProje.Application.Features.Products.Validators;

public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
