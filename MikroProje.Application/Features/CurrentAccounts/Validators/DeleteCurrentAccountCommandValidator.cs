using FluentValidation;
using MikroProje.Application.Features.CurrentAccounts.Commands.DeleteCurrentAccount;

namespace MikroProje.Application.Features.CurrentAccounts.Validators;

public class DeleteCurrentAccountCommandValidator : AbstractValidator<DeleteCurrentAccountCommand>
{
    public DeleteCurrentAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
