using FluentValidation;
using MikroProje.Application.Features.CurrentAccounts.Queries.GetAllCurrentAccounts;

namespace MikroProje.Application.Features.CurrentAccounts.Validators;

public class GetAllCurrentAccountsQueryValidator : AbstractValidator<GetAllCurrentAccountsQuery>
{
    public GetAllCurrentAccountsQueryValidator()
    {
    }
}
