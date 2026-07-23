using FluentValidation;
using MikroProje.Application.Features.Products.Queries.GetAllProducts;

namespace MikroProje.Application.Features.Products.Validators;

public class GetAllProductsQueryValidator : AbstractValidator<GetAllProductsQuery>
{
    public GetAllProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
