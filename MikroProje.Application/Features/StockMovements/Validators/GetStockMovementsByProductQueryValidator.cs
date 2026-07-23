using FluentValidation;
using MikroProje.Application.Features.StockMovements.Queries.GetStockMovementsByProduct;

namespace MikroProje.Application.Features.StockMovements.Validators;

public class GetStockMovementsByProductQueryValidator : AbstractValidator<GetStockMovementsByProductQuery>
{
    public GetStockMovementsByProductQueryValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
