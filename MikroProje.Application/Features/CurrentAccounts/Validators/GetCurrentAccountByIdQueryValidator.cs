using FluentValidation;
using MikroProje.Application.Features.CurrentAccounts.Queries.GetCurrentAccountById;

namespace MikroProje.Application.Features.CurrentAccounts.Validators;

public class GetCurrentAccountByIdQueryValidator : AbstractValidator<GetCurrentAccountByIdQuery>
{
    public GetCurrentAccountByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
