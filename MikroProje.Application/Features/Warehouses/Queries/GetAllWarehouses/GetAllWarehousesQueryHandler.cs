using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Interfaces;
using MikroProje.Application.Features.Warehouses.DTOs;

namespace MikroProje.Application.Features.Warehouses.Queries.GetAllWarehouses;

public class GetAllWarehousesQueryHandler : IRequestHandler<GetAllWarehousesQuery, Result<PagedResult<WarehouseListDto>>>
{
    private readonly IWarehouseRepository _repository;
    private readonly IMapper _mapper;

    public GetAllWarehousesQueryHandler(IWarehouseRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<WarehouseListDto>>> Handle(GetAllWarehousesQuery request, CancellationToken cancellationToken)
    {
        var pagedWarehouses = await _repository.GetAllPagedAsync(
            request.Search,
            request.IsActive,
            request.IsDefault,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtoList = _mapper.Map<List<WarehouseListDto>>(pagedWarehouses.Items);
        
        var pagedResult = PagedResult<WarehouseListDto>.Create(
            dtoList, 
            pagedWarehouses.PageNumber, 
            pagedWarehouses.PageSize,
            pagedWarehouses.TotalCount);

        return Result<PagedResult<WarehouseListDto>>.Ok(pagedResult);
    }
}
