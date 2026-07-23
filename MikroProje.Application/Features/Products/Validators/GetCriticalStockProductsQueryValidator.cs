using FluentValidation;
using MikroProje.Application.Features.Products.Queries.GetCriticalStockProducts;

namespace MikroProje.Application.Features.Products.Validators;

public class GetCriticalStockProductsQueryValidator : AbstractValidator<GetCriticalStockProductsQuery>
{
    public GetCriticalStockProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
