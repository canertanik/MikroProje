using FluentValidation;
using MikroProje.Application.Features.Payments.Commands.DeletePayment;

namespace MikroProje.Application.Features.Payments.Validators;

public class DeletePaymentCommandValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeletePaymentCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id sıfırdan büyük olmalıdır.");
    }
}
