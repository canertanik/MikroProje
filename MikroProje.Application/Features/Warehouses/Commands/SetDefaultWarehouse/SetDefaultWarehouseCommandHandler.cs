using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Interfaces;
using MikroProje.Application.Features.Warehouses.DTOs;

namespace MikroProje.Application.Features.Warehouses.Commands.SetDefaultWarehouse;

public class SetDefaultWarehouseCommandHandler : IRequestHandler<SetDefaultWarehouseCommand, Result<WarehouseDto>>
{
    private readonly IWarehouseRepository _repository;
    private readonly IMapper _mapper;

    public SetDefaultWarehouseCommandHandler(IWarehouseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<WarehouseDto>> Handle(SetDefaultWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse == null)
        {
            return Result<WarehouseDto>.Fail("Depo bulunamadý.", 404);
        }

        if (!warehouse.IsActive)
        {
            return Result<WarehouseDto>.Fail("Pasif bir depo varsayýlan olarak ayarlanamaz.", 400);
        }

        if (warehouse.IsDefault)
        {
            return Result<WarehouseDto>.Fail("Bu depo zaten varsayýlan depo olarak ayarlý.", 400);
        }

        var currentDefault = await _repository.GetDefaultWarehouseAsync(cancellationToken);
        if (currentDefault != null)
        {
            currentDefault.IsDefault = false;
            currentDefault.UpdatedDate = DateTime.UtcNow;
        }

        warehouse.IsDefault = true;
        warehouse.UpdatedDate = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);

        var resultDto = _mapper.Map<WarehouseDto>(warehouse);
        return Result<WarehouseDto>.Ok(resultDto, "Varsayýlan depo deðiþtirildi.");
    }
}
