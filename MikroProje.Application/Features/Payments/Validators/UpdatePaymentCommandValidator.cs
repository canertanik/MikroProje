using FluentValidation;
using MikroProje.Application.Features.Payments.Commands.UpdatePayment;

namespace MikroProje.Application.Features.Payments.Validators;

public class UpdatePaymentCommandValidator : AbstractValidator<UpdatePaymentCommand>
{
    public UpdatePaymentCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id sıfırdan büyük olmalıdır.");

        RuleFor(x => x.CurrentAccountId)
            .GreaterThan(0).WithMessage("Geçerli bir cari hesap seçilmelidir.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Tutar sıfırdan büyük olmalıdır.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Tahsilat tarihi gereklidir.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Geçerli bir ödeme yöntemi seçilmelidir.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("RowVersion gereklidir.");
    }
}
