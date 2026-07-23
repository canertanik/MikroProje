using AutoMapper;
using MikroProje.Application.Features.Products.DTOs;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.Products.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(destination => destination.IsCriticalStock,
                options => options.MapFrom(source => source.StockQuantity <= source.CriticalStockQuantity));
    }
}
