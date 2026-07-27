using FluentValidation;

namespace MikroProje.Application.Features.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
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
