using FluentValidation;

namespace MikroProje.Application.Features.Purchases.Queries.GetAllPurchases;

public class GetAllPurchasesQueryValidator : AbstractValidator<GetAllPurchasesQuery>
{
    public GetAllPurchasesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100.");
    }
}
