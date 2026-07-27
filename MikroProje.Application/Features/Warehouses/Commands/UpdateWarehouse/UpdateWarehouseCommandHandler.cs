using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Interfaces;
using MikroProje.Application.Features.Warehouses.DTOs;
using MikroProje.Application.Common.Exceptions;

namespace MikroProje.Application.Features.Warehouses.Commands.UpdateWarehouse;

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, Result<WarehouseDto>>
{
    private readonly IWarehouseRepository _repository;
    private readonly IMapper _mapper;

    public UpdateWarehouseCommandHandler(IWarehouseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<WarehouseDto>> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (request.Id != dto.Id)
        {
            return Result<WarehouseDto>.Fail("ID uyuþmazlýðý.", 400);
        }

        var warehouse = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse == null)
        {
            return Result<WarehouseDto>.Fail("Depo bulunamadý.", 404);
        }

        string code = dto.Code.Trim().ToUpper();

        if (await _repository.CodeExistsAsync(code, warehouse.Id, cancellationToken))
        {
            return Result<WarehouseDto>.Fail("Bu depo kodu baþka bir depo tarafýndan kullanýlýyor.", 409);
        }

        if (warehouse.IsDefault && !dto.IsActive)
        {
            return Result<WarehouseDto>.Fail("Varsayýlan depo pasif duruma getirilemez. Önce baþka bir depoyu varsayýlan yapmalýsýnýz.", 400);
        }

        warehouse.Code = code;
        warehouse.Name = dto.Name.Trim();
        warehouse.Description = dto.Description;
        warehouse.IsActive = dto.IsActive;

        try
        {
            await _repository.UpdateAsync(warehouse, dto.RowVersion, cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Result<WarehouseDto>.Fail("Kayýt baþka bir kullanýcý tarafýndan güncellenmiþ. Lütfen sayfayý yenileyerek tekrar deneyin.", 409);
        }

        var resultDto = _mapper.Map<WarehouseDto>(warehouse);
        return Result<WarehouseDto>.Ok(resultDto, "Depo baþarýyla güncellendi.");
    }
}
