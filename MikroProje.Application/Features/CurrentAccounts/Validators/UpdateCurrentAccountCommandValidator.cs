using FluentValidation;
using MikroProje.Application.Features.CurrentAccounts.Commands.UpdateCurrentAccount;

namespace MikroProje.Application.Features.CurrentAccounts.Validators;

public class UpdateCurrentAccountCommandValidator : AbstractValidator<UpdateCurrentAccountCommand>
{
    public UpdateCurrentAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(30)
            .When(x => x.Phone is not null);
    }
}
