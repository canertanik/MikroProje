using FluentValidation;

namespace MikroProje.Application.Features.SupplierStatements.Queries.GetSupplierStatement;

public class GetSupplierStatementQueryValidator : AbstractValidator<GetSupplierStatementQuery>
{
    public GetSupplierStatementQueryValidator()
    {
        RuleFor(x => x.CurrentAccountId)
            .GreaterThan(0).WithMessage("Geçerli bir cari hesap seçilmelidir.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Sayfa numarası 1 veya daha büyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır.");

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate.Value <= x.EndDate.Value)
            .WithMessage("Başlangıç tarihi, bitiş tarihinden büyük olamaz.");
    }
}
