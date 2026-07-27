using AutoMapper;
using MikroProje.Application.Features.Warehouses.DTOs;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.Warehouses.Mappings;

public class WarehouseProfile : Profile
{
    public WarehouseProfile()
    {
        CreateMap<Warehouse, WarehouseDto>();
        CreateMap<Warehouse, WarehouseListDto>();
    }
}
