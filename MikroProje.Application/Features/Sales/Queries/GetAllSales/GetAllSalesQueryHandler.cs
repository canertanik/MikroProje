using AutoMapper;
using MediatR;
using MikroProje.Application.Common.Pagination;
using MikroProje.Application.Common.Results;
using MikroProje.Application.Features.Sales.DTOs;
using MikroProje.Application.Interfaces;

namespace MikroProje.Application.Features.Sales.Queries.GetAllSales;

public class GetAllSalesQueryHandler : IRequestHandler<GetAllSalesQuery, Result<PagedResult<SaleDto>>>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public GetAllSalesQueryHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<SaleDto>>> Handle(GetAllSalesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _saleRepository.GetAllAsync(
            request.Search, request.PageNumber, request.PageSize, cancellationToken);
        var dtos = _mapper.Map<IReadOnlyCollection<SaleDto>>(items);
        var pagedResult = PagedResult<SaleDto>.Create(dtos, request.PageNumber, request.PageSize, totalCount);
        return Result<PagedResult<SaleDto>>.Ok(pagedResult);
    }
}
