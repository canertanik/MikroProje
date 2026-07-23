using AutoMapper;
using MikroProje.Application.Features.StockMovements.DTOs;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.StockMovements.Mappings;

public class StockMovementProfile : Profile
{
    public StockMovementProfile()
    {
        CreateMap<StockMovement, StockMovementDto>()
            .ForMember(destination => destination.ProductCode, options => options.MapFrom(source => source.Product.Code))
            .ForMember(destination => destination.ProductName, options => options.MapFrom(source => source.Product.Name));
    }
}
