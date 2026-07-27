using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Interfaces;
using MikroProje.Domain.Entities;
using MikroProje.Application.Features.Warehouses.DTOs;

namespace MikroProje.Application.Features.Warehouses.Commands.CreateWarehouse;

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Result<WarehouseDto>>
{
    private readonly IWarehouseRepository _repository;
    private readonly IMapper _mapper;

    public CreateWarehouseCommandHandler(IWarehouseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<WarehouseDto>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        string code = dto.Code.Trim().ToUpper();
        
        if (await _repository.CodeExistsAsync(code, null, cancellationToken))
        {
            return Result<WarehouseDto>.Fail("Bu depo kodu zaten kullanýlýyor.", 409);
        }

        if (dto.IsDefault && !dto.IsActive)
        {
            return Result<WarehouseDto>.Fail("Pasif bir depo varsayýlan olarak ayarlanamaz.", 400);
        }

        if (dto.IsDefault)
        {
            var defaultWarehouse = await _repository.GetDefaultWarehouseAsync(cancellationToken);
            if (defaultWarehouse != null)
            {
                defaultWarehouse.IsDefault = false;
            }
        }

        var warehouse = new Warehouse
        {
            Code = code,
            Name = dto.Name.Trim(),
            Description = dto.Description,
            IsDefault = dto.IsDefault,
            IsActive = dto.IsActive,
            CreatedDate = DateTime.UtcNow
        };

        await _repository.AddAsync(warehouse, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var resultDto = _mapper.Map<WarehouseDto>(warehouse);
        return Result<WarehouseDto>.Created(resultDto, "Depo baþarýyla oluþturuldu.");
    }
}
