using AutoMapper;
using MikroProje.Application.Features.CurrentAccounts.DTOs;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.CurrentAccounts.Mappings;

public class CurrentAccountProfile : Profile
{
    public CurrentAccountProfile()
    {
        CreateMap<CurrentAccount, CurrentAccountDto>();
    }
}
