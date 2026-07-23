using FluentValidation;

namespace MikroProje.Application.Features.SupplierPayments.Queries.GetSupplierPaymentById;

public class GetSupplierPaymentByIdQueryValidator : AbstractValidator<GetSupplierPaymentByIdQuery>
{
    public GetSupplierPaymentByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Geçerli bir ID girilmelidir.");
    }
}
