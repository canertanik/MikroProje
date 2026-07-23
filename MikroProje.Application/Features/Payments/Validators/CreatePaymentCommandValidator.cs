using FluentValidation;
using MikroProje.Application.Features.Payments.Commands.CreatePayment;

namespace MikroProje.Application.Features.Payments.Validators;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.CurrentAccountId)
            .GreaterThan(0).WithMessage("Geçerli bir cari hesap seçilmelidir.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Tutar 0'dan büyük olmalıdır.");

        RuleFor(x => x.PaymentDate)
            .NotEmpty().WithMessage("Tarih boş olamaz.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Geçerli bir ödeme yöntemi seçilmelidir.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Geçerli bir işlem tipi seçilmelidir.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}
