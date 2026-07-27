using FluentValidation;

namespace MikroProje.Application.Features.StockTransfers.Queries.GetAllStockTransfers;

public class GetAllStockTransfersQueryValidator : AbstractValidator<GetAllStockTransfersQuery>
{
    public GetAllStockTransfersQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage("Sayfa numarasý en az 1 olmalýdýr.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("Sayfa boyutu 1 ile 100 arasýnda olmalýdýr.");
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Baþlangýç tarihi bitiþ tarihinden büyük olamaz.");
    }
}
