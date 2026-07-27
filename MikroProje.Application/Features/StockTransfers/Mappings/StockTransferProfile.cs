using AutoMapper;
using MikroProje.Application.Features.StockTransfers.DTOs;
using MikroProje.Domain.Entities;

namespace MikroProje.Application.Features.StockTransfers.Mappings;

public class StockTransferProfile : Profile
{
    public StockTransferProfile()
    {
        // Add mapping logic here if needed
        // The current queries use direct Select projection, so Automapper is not strictly necessary for Read.
        // But we can add standard maps just in case.
        CreateMap<StockTransfer, StockTransferDto>();
        CreateMap<StockTransfer, StockTransferListDto>();
        CreateMap<StockTransferItem, StockTransferItemDto>();
    }
}
