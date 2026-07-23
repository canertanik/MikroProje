using FluentValidation;

namespace MikroProje.Application.Features.SupplierPayments.Commands.CreateSupplierPayment;

public class CreateSupplierPaymentCommandValidator : AbstractValidator<CreateSupplierPaymentCommand>
{
    public CreateSupplierPaymentCommandValidator()
    {
        RuleFor(x => x.CurrentAccountId)
            .GreaterThan(0).WithMessage("Geçerli bir cari hesap ID'si girilmelidir.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Ödeme tutarı sıfırdan büyük olmalıdır.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Geçersiz ödeme yöntemi.");

        RuleFor(x => x.ReferenceNumber)
            .MaximumLength(50).WithMessage("Referans numarası en fazla 50 karakter olabilir.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.");
    }
}
