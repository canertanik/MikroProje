using FluentValidation;

namespace MikroProje.Application.Features.AuditLogs.Queries.GetAllAuditLogs;

public class GetAllAuditLogsQueryValidator : AbstractValidator<GetAllAuditLogsQuery>
{
    public GetAllAuditLogsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("Sayfa numarasý 1 veya daha büyük olmalýdýr.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1 ile 100 arasýnda olmalýdýr.");
    }
}
