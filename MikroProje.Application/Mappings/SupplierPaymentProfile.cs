using AutoMapper;
using MikroProje.Application.Features.SupplierPayments.DTOs;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Mappings;

public class SupplierPaymentProfile : Profile
{
    public SupplierPaymentProfile()
    {
        CreateMap<SupplierPayment, SupplierPaymentDto>()
            .ForMember(dest => dest.CurrentAccountName, opt => opt.MapFrom(src => src.CurrentAccount.Name));

        CreateMap<SupplierPayment, SupplierPaymentListDto>()
            .ForMember(dest => dest.CurrentAccountName, opt => opt.MapFrom(src => src.CurrentAccount.Name));
    }
}
