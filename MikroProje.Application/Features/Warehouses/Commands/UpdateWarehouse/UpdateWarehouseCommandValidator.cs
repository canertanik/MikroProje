using FluentValidation;

namespace MikroProje.Application.Features.Warehouses.Commands.UpdateWarehouse;

public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Geçerli bir depo ID'si girilmelidir.");

        RuleFor(v => v.Dto.Id)
            .GreaterThan(0).WithMessage("Geçerli bir depo ID'si girilmelidir.")
            .Equal(v => v.Id).WithMessage("ID değerleri eşleşmiyor.");

        RuleFor(v => v.Dto.Code)
            .NotEmpty().WithMessage("Depo kodu boş olamaz.")
            .MaximumLength(50).WithMessage("Depo kodu en fazla 50 karakter olmalıdır.");

        RuleFor(v => v.Dto.Name)
            .NotEmpty().WithMessage("Depo adı boş olamaz.")
            .MaximumLength(100).WithMessage("Depo adı en fazla 100 karakter olmalıdır.");

        RuleFor(v => v.Dto.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olmalıdır.");
    }
}
