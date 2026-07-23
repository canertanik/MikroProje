using FluentValidation;

namespace MikroProje.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Request.FirstName)
            .NotEmpty().WithMessage("Ad boş olamaz.");

        RuleFor(x => x.Request.LastName)
            .NotEmpty().WithMessage("Soyad boş olamaz.");

        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email boş olamaz.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");

        RuleFor(x => x.Request.Password)
            .NotEmpty().WithMessage("Parola boş olamaz.")
            .MinimumLength(8).WithMessage("Parola en az 8 karakter olmalıdır.")
            .Matches("[A-Z]").WithMessage("Parola en az bir büyük harf içermelidir.")
            .Matches("[a-z]").WithMessage("Parola en az bir küçük harf içermelidir.")
            .Matches("[0-9]").WithMessage("Parola en az bir rakam içermelidir.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Parola en az bir özel karakter içermelidir.");

        RuleFor(x => x.Request.ConfirmPassword)
            .Equal(x => x.Request.Password).WithMessage("Parolalar eşleşmiyor.");
    }
}
