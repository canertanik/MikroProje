using FluentValidation;

namespace MikroProje.Application.Features.SupplierPayments.Queries.GetAllSupplierPayments;

public class GetAllSupplierPaymentsQueryValidator : AbstractValidator<GetAllSupplierPaymentsQuery>
{
    public GetAllSupplierPaymentsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber 1 veya daha büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize 1 ile 100 arasında olmalıdır.");
    }
}
