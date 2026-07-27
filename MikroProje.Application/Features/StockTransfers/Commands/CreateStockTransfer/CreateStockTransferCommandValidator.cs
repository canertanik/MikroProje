using FluentValidation;

namespace MikroProje.Application.Features.StockTransfers.Commands.CreateStockTransfer;

public class CreateStockTransferCommandValidator : AbstractValidator<CreateStockTransferCommand>
{
    public CreateStockTransferCommandValidator()
    {
        RuleFor(x => x.Dto.SourceWarehouseId).GreaterThan(0).WithMessage("Kaynak depo seçilmelidir.");
        RuleFor(x => x.Dto.DestinationWarehouseId).GreaterThan(0).WithMessage("Hedef depo seçilmelidir.");
        RuleFor(x => x.Dto.SourceWarehouseId).NotEqual(x => x.Dto.DestinationWarehouseId).WithMessage("Kaynak ve hedef depo ayný olamaz.");
        RuleFor(x => x.Dto.Items).NotEmpty().WithMessage("Transfer en az bir kalem içermelidir.");
        
        RuleForEach(x => x.Dto.Items).ChildRules(item => 
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0).WithMessage("Geçerli bir ürün seçilmelidir.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Miktar sýfýrdan büyük olmalýdýr.");
        });
    }
}
