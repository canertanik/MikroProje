using AutoMapper;
using MikroProje.Application.Features.Purchases.DTOs;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Mappings;

public class PurchaseProfile : Profile
{
    public PurchaseProfile()
    {
        CreateMap<Purchase, PurchaseDto>()
            .ForMember(dest => dest.CurrentAccountName, opt => opt.MapFrom(src => src.CurrentAccount != null ? src.CurrentAccount.Name : string.Empty))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty));

        CreateMap<PurchaseItem, PurchaseItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

        CreateMap<Purchase, PurchaseListDto>()
            .ForMember(dest => dest.CurrentAccountName, opt => opt.MapFrom(src => src.CurrentAccount != null ? src.CurrentAccount.Name : string.Empty));
    
        
    }
}


