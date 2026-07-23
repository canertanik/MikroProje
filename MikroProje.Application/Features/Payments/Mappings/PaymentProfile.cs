using AutoMapper;
using MikroProje.Application.Features.Payments.DTOs;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.Payments.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Payment, PaymentDto>()
            .ForMember(d => d.CurrentAccountName, o => o.MapFrom(s => s.CurrentAccount != null ? s.CurrentAccount.Name : string.Empty))
            .ForMember(d => d.CurrentAccountCode, o => o.MapFrom(s => s.CurrentAccount != null ? s.CurrentAccount.Code : string.Empty))
            .ForMember(d => d.TypeName, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.PaymentMethodName, o => o.MapFrom(s => s.PaymentMethod.ToString()));
    }
}
