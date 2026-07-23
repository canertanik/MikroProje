using FluentValidation;

namespace MikroProje.Application.Features.CurrentAccounts.Queries.GetStatement;

public class GetCurrentAccountStatementQueryValidator : AbstractValidator<GetCurrentAccountStatementQuery>
{
    public GetCurrentAccountStatementQueryValidator()
    {
        RuleFor(x => x.CurrentAccountId)
            .GreaterThan(0).WithMessage("CurrentAccountId must be greater than 0.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100.");
            
        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("StartDate cannot be later than EndDate.");
    }
}
