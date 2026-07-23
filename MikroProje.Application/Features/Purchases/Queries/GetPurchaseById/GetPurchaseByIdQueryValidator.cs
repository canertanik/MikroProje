using FluentValidation;

namespace MikroProje.Application.Features.Purchases.Queries.GetPurchaseById;

public class GetPurchaseByIdQueryValidator : AbstractValidator<GetPurchaseByIdQuery>
{
    public GetPurchaseByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçerli bir Id girilmelidir.");
    }
}
