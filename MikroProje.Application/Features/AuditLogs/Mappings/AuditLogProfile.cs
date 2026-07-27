using AutoMapper;
using MikroProje.Application.Features.AuditLogs.DTOs;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.AuditLogs.Mappings;

public class AuditLogProfile : Profile
{
    public AuditLogProfile()
    {
        CreateMap<AuditLog, AuditLogDto>();
        CreateMap<AuditLog, AuditLogListDto>();
    
        
    }
}


