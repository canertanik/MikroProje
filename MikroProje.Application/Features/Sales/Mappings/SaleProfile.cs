using AutoMapper;
using MikroProje.Application.Features.Sales.DTOs;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.Sales.Mappings;

public class SaleProfile : Profile
{
    public SaleProfile()
    {
        // Sale → SaleDto
        CreateMap<Sale, SaleDto>()
            .ForMember(dest => dest.CurrentAccountName,
                opt => opt.MapFrom(src => src.CurrentAccount != null ? src.CurrentAccount.Name : string.Empty))
            .ForMember(dest => dest.CurrentAccountCode,
                opt => opt.MapFrom(src => src.CurrentAccount != null ? src.CurrentAccount.Code : string.Empty))
            .ForMember(dest => dest.WarehouseName,
                opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.Details));

        // SaleDetail → SaleDetailDto
        CreateMap<SaleDetail, SaleDetailDto>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.ProductCode,
                opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty));
    
        
    }
}


